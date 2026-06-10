using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Constants;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;
using SmartEducation.Persistence.Contexts;

namespace SmartEducation.Web.Areas.Parent.Controllers
{
    [Area("Parent")]
    [Authorize(Roles = Roles.Parent)]
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IUnitOfWork     _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public MessageController(
            IMessageService messageService,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext)
        {
            _messageService = messageService;
            _unitOfWork     = unitOfWork;
            _userManager    = userManager;
            _dbContext      = dbContext;
        }

        // ── Main chat page ──────────────────────────────────────────────────

        public async Task<IActionResult> Index(Guid? with = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });
            ViewBag.CurrentUserId = user.Id;
            ViewBag.OpenWith      = with;
            return View();
        }

        // ── AJAX: conversations list ────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var convs = await _messageService.GetConversationsAsync(user.Id);
            return Json(convs);
        }

        // ── AJAX: messages in a conversation ───────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetMessages(Guid otherId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            await _messageService.MarkConversationReadAsync(user.Id, otherId);
            var messages = await _messageService.GetConversationAsync(user.Id, otherId);
            return Json(messages);
        }

        // ── AJAX: send a chat message ───────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send([FromBody] ParentSendChatRequest req)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            if (string.IsNullOrWhiteSpace(req.Body) || req.ReceiverId == Guid.Empty)
                return BadRequest(new { error = "Invalid message" });

            var msg = await _messageService.SendChatAsync(user.Id, req.ReceiverId, req.Body.Trim());
            return Json(msg);
        }

        // ── AJAX: get teachers for this parent's children ──────────────────
        // Supports both junction-table and email-based linking.

        [HttpGet]
        public async Task<IActionResult> GetMyTeachers()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var children = await FindChildrenForParent(user.Id, user.Email);
            if (!children.Any())
                return Json(Array.Empty<object>());

            var assignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var allProfiles = await _unitOfWork.TeacherProfiles.GetAllAsync();
            var allUsers    = _userManager.Users.ToList();
            var classRooms  = await _unitOfWork.ClassRooms.GetAllAsync();

            var teacherSet = new HashSet<Guid>();
            var result = new List<object>();

            foreach (var child in children)
            {
                if (!child.ClassRoomId.HasValue) continue;

                var classId = child.ClassRoomId.Value;
                var cr = classRooms.FirstOrDefault(c => c.Id == classId);

                var teacherIds = assignments
                    .Where(ta => ta.ClassRoomId == classId && ta.IsActive)
                    .Select(ta => ta.TeacherId)
                    .Distinct();

                foreach (var tid in teacherIds)
                {
                    if (!teacherSet.Add(tid)) continue; // skip duplicates across children

                    var tp = allProfiles.FirstOrDefault(p => p.Id == tid);
                    if (tp == null) continue;
                    var tu = allUsers.FirstOrDefault(u => u.Id == tp.UserId);
                    if (tu == null) continue;

                    result.Add(new
                    {
                        userId         = tu.Id,
                        name           = $"{tu.FirstName} {tu.LastName}".Trim(),
                        specialization = tp.Specialization ?? "",
                        className      = cr?.Name ?? ""
                    });
                }
            }

            return Json(result);
        }

        // ── AJAX: get contact info for a user ──────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetContact(Guid userId)
        {
            var otherUser = await _userManager.FindByIdAsync(userId.ToString());
            if (otherUser == null) return NotFound();

            var roles  = await _userManager.GetRolesAsync(otherUser);
            var role   = roles.FirstOrDefault() ?? "";
            var initials = SafeInitials(otherUser.FirstName, otherUser.LastName);

            string specialization = "";
            if (role == Roles.Teacher)
            {
                var teacherProfiles = await _unitOfWork.TeacherProfiles.GetAllAsync();
                var tp = teacherProfiles.FirstOrDefault(p => p.UserId == userId);
                if (tp != null) specialization = tp.Specialization ?? "";
            }

            return Json(new
            {
                userId,
                fullName       = $"{otherUser.FirstName} {otherUser.LastName}".Trim(),
                role,
                initials,
                specialization,
                email = otherUser.Email ?? ""
            });
        }

        // ── AJAX: child's academic context ─────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetChildContext()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var children = await FindChildrenForParent(user.Id, user.Email);

            if (!children.Any())
            {
                return Json(new
                {
                    hasData = false,
                    message = "No linked student found. Ask the school administrator to link your child's profile."
                });
            }

            // Return context for the first (or most recently enrolled) child
            var child = children
                .OrderByDescending(c => c.EnrollmentDate)
                .First();

            var ctx = await BuildChildContext(child);
            return Json(new { hasData = true, context = ctx });
        }

        // ══════════════════════════════════════════════════════════════════
        //  HELPERS
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Finds all student profiles linked to this parent.
        /// Strategy 1: ParentStudents junction table (formal link).
        /// Strategy 2: StudentProfile.ParentEmail field (legacy/fallback).
        /// </summary>
        private async Task<List<StudentProfile>> FindChildrenForParent(Guid parentUserId, string? parentEmail)
        {
            var allStudents = await _unitOfWork.StudentProfiles.GetAllAsync();
            var found = new List<StudentProfile>();

            // Strategy 1: junction table
            var parentProfiles = await _unitOfWork.ParentProfiles.GetAllAsync();
            var parentProfile  = parentProfiles.FirstOrDefault(p => p.UserId == parentUserId);

            if (parentProfile != null)
            {
                var links = _dbContext.ParentStudents
                    .Where(ps => ps.ParentId == parentProfile.Id)
                    .Select(ps => ps.StudentId)
                    .ToHashSet();

                if (links.Any())
                {
                    found.AddRange(allStudents.Where(s => links.Contains(s.Id)));
                    return found; // junction table is authoritative
                }
            }

            // Strategy 2: email field fallback
            if (!string.IsNullOrWhiteSpace(parentEmail))
            {
                var byEmail = allStudents.Where(s =>
                    s.ParentEmail != null &&
                    s.ParentEmail.Equals(parentEmail, StringComparison.OrdinalIgnoreCase));
                found.AddRange(byEmail);
            }

            return found;
        }

        private async Task<StudentContextDto> BuildChildContext(StudentProfile sp)
        {
            var allUsers   = _userManager.Users.ToList();
            var childUser  = allUsers.FirstOrDefault(u => u.Id == sp.UserId);
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var cr = sp.ClassRoomId.HasValue
                ? classRooms.FirstOrDefault(c => c.Id == sp.ClassRoomId.Value)
                : null;

            var studentExams = await _unitOfWork.StudentExams.GetAllAsync();
            var sExams       = studentExams.Where(se => se.StudentId == sp.Id && se.IsSubmitted).ToList();
            var avgScore     = sExams.Any() ? Math.Round(sExams.Average(e => e.Score), 1) : 0;

            var exams     = await _unitOfWork.Exams.GetAllAsync();
            var latestSE  = sExams.OrderByDescending(se =>
                exams.FirstOrDefault(e => e.Id == se.ExamId)?.ExamDate ?? DateTime.MinValue).FirstOrDefault();
            var latestExam = latestSE != null ? exams.FirstOrDefault(e => e.Id == latestSE.ExamId) : null;

            var assignments    = await _unitOfWork.Assignments.GetAllAsync();
            var classAssignIds = assignments.Where(a => a.ClassRoomId == sp.ClassRoomId).Select(a => a.Id).ToList();
            var submissions    = await _unitOfWork.AssignmentSubmissions.GetAllAsync();
            var submitted      = submissions.Count(s => s.StudentId == sp.Id && classAssignIds.Contains(s.AssignmentId));
            var assignRate     = classAssignIds.Count > 0
                ? Math.Round((double)submitted / classAssignIds.Count * 100, 1) : 0;
            var pending = Math.Max(0, classAssignIds.Count - submitted);

            var attendanceSessions = await _unitOfWork.AttendanceSessions.GetAllAsync();
            var attendanceRecords  = await _unitOfWork.AttendanceRecords.GetAllAsync();
            var crSessions  = attendanceSessions.Where(s => s.ClassRoomId == sp.ClassRoomId).Select(s => s.Id).ToList();
            var sAttendance = attendanceRecords.Where(r => r.StudentId == sp.Id && crSessions.Contains(r.AttendanceSessionId)).ToList();
            var presentCount = sAttendance.Count(r => r.IsPresent);
            var attendRate   = sAttendance.Count > 0
                ? Math.Round((double)presentCount / sAttendance.Count * 100, 1) : 100;
            var absenceCount = sAttendance.Count(r => !r.IsPresent);

            var score = (avgScore * 0.5) + (attendRate * 0.3) + (assignRate * 0.2);
            string level, color, emoji;
            if      (score >= 80) { level = "Excellent";         color = "#71dd37"; emoji = "🟢"; }
            else if (score >= 65) { level = "Good";              color = "#696cff"; emoji = "🔵"; }
            else if (score >= 50) { level = "Needs Improvement"; color = "#fd7e14"; emoji = "🟠"; }
            else                  { level = "At Risk";            color = "#e74c3c"; emoji = "🔴"; }

            var alerts = new List<string>();
            if (attendRate < 75)            alerts.Add($"⚠️ Attendance critically low ({attendRate}%)");
            if (avgScore < 50 && sExams.Any()) alerts.Add($"⚠️ Exam average below passing ({avgScore}%)");
            if (pending > 0)                alerts.Add($"📋 {pending} assignment(s) pending submission");
            if (absenceCount >= 5)          alerts.Add($"📅 {absenceCount} absences recorded");

            return new StudentContextDto
            {
                ProfileId                = sp.Id,
                FullName                 = childUser != null ? $"{childUser.FirstName} {childUser.LastName}".Trim() : "Unknown",
                StudentNumber            = sp.StudentNumber ?? "N/A",
                ClassName                = cr?.Name ?? "Not assigned",
                AttendanceRate           = attendRate,
                AbsenceCount             = absenceCount,
                LatestExamGrade          = latestSE?.Score ?? 0,
                LatestExamTotal          = latestExam?.TotalMarks ?? 0,
                LatestExamTitle          = latestExam?.Title ?? "—",
                AverageGrade             = avgScore,
                AssignmentCompletionRate = assignRate,
                PendingAssignments       = pending,
                PerformanceLevel         = level,
                PerformanceColor         = color,
                PerformanceEmoji         = emoji,
                Alerts                   = alerts
            };
        }

        private static string SafeInitials(string first, string last)
        {
            var a = first?.Length > 0 ? first[0].ToString() : "?";
            var b = last?.Length  > 0 ? last[0].ToString()  : "";
            return (a + b).ToUpperInvariant();
        }
    }

    public class ParentSendChatRequest
    {
        public Guid   ReceiverId { get; set; }
        public string Body       { get; set; } = string.Empty;
    }
}
