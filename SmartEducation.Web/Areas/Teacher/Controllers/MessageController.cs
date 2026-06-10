using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Constants;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Web.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = Roles.Teacher)]
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly ITeacherService _teacherService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessageController(IMessageService messageService, ITeacherService teacherService,
            IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _messageService = messageService;
            _teacherService = teacherService;
            _unitOfWork     = unitOfWork;
            _userManager    = userManager;
        }

        private async Task<(ApplicationUser user, TeacherProfile? profile)> GetCurrentUser()
        {
            var user    = await _userManager.GetUserAsync(User);
            var profile = user != null ? await _teacherService.GetProfileByUserIdAsync(user.Id) : null;
            return (user!, profile);
        }

        // ── Main chat page ───────────────────────────────────────────────────

        public async Task<IActionResult> Index(Guid? with = null)
        {
            var (user, _) = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

            ViewBag.CurrentUserId = user.Id;
            ViewBag.OpenWith      = with;
            return View();
        }

        // ── AJAX: conversations list ─────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var conversations = await _messageService.GetConversationsAsync(user.Id);
            return Json(conversations);
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
        public async Task<IActionResult> Send([FromBody] SendChatRequest req)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(req.Body) || req.ReceiverId == Guid.Empty)
                return BadRequest(new { error = "Invalid message" });

            var msg = await _messageService.SendChatAsync(user.Id, req.ReceiverId, req.Body.Trim());
            return Json(msg);
        }

        // ── AJAX: mark conversation read ────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead([FromBody] Guid otherId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            await _messageService.MarkConversationReadAsync(user.Id, otherId);
            return Ok();
        }

        // ── AJAX: student academic context panel ────────────────────────────

        [HttpGet]
        public async Task<IActionResult> StudentContext(Guid studentProfileId)
        {
            var ctx = await BuildStudentContext(studentProfileId);
            if (ctx == null) return NotFound();
            return Json(ctx);
        }

        // ── AJAX: get contact info for a user (to populate conversation header) ─

        [HttpGet]
        public async Task<IActionResult> GetContact(Guid userId)
        {
            var otherUser = await _userManager.FindByIdAsync(userId.ToString());
            if (otherUser == null) return NotFound();

            var roles    = await _userManager.GetRolesAsync(otherUser);
            var role     = roles.FirstOrDefault() ?? "";
            var initials = (otherUser.FirstName.Length > 0 ? otherUser.FirstName[0].ToString() : "?") +
                           (otherUser.LastName.Length  > 0 ? otherUser.LastName[0].ToString()  : "");

            // If user is student, find their profile
            Guid? studentProfileId = null;
            string studentNumber = "";
            string className     = "";
            Guid? parentUserId   = null;

            if (role == "Student")
            {
                var studentProfiles = await _unitOfWork.StudentProfiles.GetAllAsync();
                var sp = studentProfiles.FirstOrDefault(s => s.UserId == userId);
                if (sp != null)
                {
                    studentProfileId = sp.Id;
                    studentNumber    = sp.StudentNumber ?? "";
                    var classRooms   = await _unitOfWork.ClassRooms.GetAllAsync();
                    className        = sp.ClassRoomId.HasValue
                        ? classRooms.FirstOrDefault(c => c.Id == sp.ClassRoomId)?.Name ?? ""
                        : "";
                    // Find linked parent
                    if (!string.IsNullOrEmpty(sp.ParentEmail))
                    {
                        var parentUser = await _userManager.FindByEmailAsync(sp.ParentEmail);
                        if (parentUser != null) parentUserId = parentUser.Id;
                    }
                }
            }
            else if (role == "Parent")
            {
                // Find the student linked to this parent by matching parent email
                var studentProfiles = await _unitOfWork.StudentProfiles.GetAllAsync();
                var sp = studentProfiles.FirstOrDefault(s =>
                    s.ParentEmail != null &&
                    s.ParentEmail.Equals(otherUser.Email, StringComparison.OrdinalIgnoreCase));
                if (sp != null)
                {
                    studentProfileId = sp.Id;
                    studentNumber    = sp.StudentNumber ?? "";
                    var classRooms   = await _unitOfWork.ClassRooms.GetAllAsync();
                    className        = sp.ClassRoomId.HasValue
                        ? classRooms.FirstOrDefault(c => c.Id == sp.ClassRoomId)?.Name ?? ""
                        : "";
                }
            }

            return Json(new
            {
                userId,
                fullName = $"{otherUser.FirstName} {otherUser.LastName}".Trim(),
                role,
                initials,
                studentProfileId,
                studentNumber,
                className,
                parentUserId,
                email = otherUser.Email ?? ""
            });
        }

        // ── AJAX: teacher's classes (new message step 1) ────────────────────

        [HttpGet]
        public async Task<IActionResult> GetMyClasses()
        {
            var (user, profile) = await GetCurrentUser();
            if (profile == null) return Unauthorized();

            var assignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var myClassIds  = assignments.Where(ta => ta.TeacherId == profile.Id)
                .Select(ta => ta.ClassRoomId).Distinct().ToList();

            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var grades     = await _unitOfWork.Grades.GetAllAsync();

            var result = myClassIds
                .Select(id =>
                {
                    var cr    = classRooms.FirstOrDefault(c => c.Id == id);
                    if (cr == null) return null;
                    var grade = grades.FirstOrDefault(g => g.Id == cr.GradeId);
                    return new { id, name = cr.Name, gradeName = grade?.Name ?? "" };
                })
                .Where(x => x != null)
                .OrderBy(x => x!.name)
                .ToList();

            return Json(result);
        }

        // ── AJAX: students for a class (new message step 2) ─────────────────

        [HttpGet]
        public async Task<IActionResult> GetStudentsForClass(Guid classId, string? search = null)
        {
            var studentProfiles = await _unitOfWork.StudentProfiles.GetAllAsync();
            var classStudents   = studentProfiles.Where(s => s.ClassRoomId == classId).ToList();
            var allUsers        = _userManager.Users.ToList();

            var result = classStudents
                .Select(sp =>
                {
                    var u = allUsers.FirstOrDefault(u => u.Id == sp.UserId);
                    if (u == null) return null;
                    var name = $"{u.FirstName} {u.LastName}".Trim();
                    return new
                    {
                        studentProfileId = sp.Id,
                        userId           = sp.UserId,
                        name,
                        studentNumber    = sp.StudentNumber ?? "",
                        parentEmail      = sp.ParentEmail   ?? "",
                        parentName       = sp.ParentName    ?? ""
                    };
                })
                .Where(x => x != null)
                .ToList();

            if (!string.IsNullOrEmpty(search))
            {
                var q = search.ToLower();
                result = result.Where(x =>
                    x!.name.ToLower().Contains(q) ||
                    x.studentNumber.ToLower().Contains(q)).ToList();
            }

            return Json(result.OrderBy(x => x!.name));
        }

        // ── AJAX: find parent user for a student ────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetParentForStudent(Guid studentProfileId)
        {
            var sp = await _unitOfWork.StudentProfiles.GetByIdAsync(studentProfileId);
            if (sp == null || string.IsNullOrEmpty(sp.ParentEmail))
                return Json(new { found = false });

            var parentUser = await _userManager.FindByEmailAsync(sp.ParentEmail);
            if (parentUser == null)
                return Json(new { found = false, parentName = sp.ParentName ?? "Parent", parentEmail = sp.ParentEmail });

            return Json(new
            {
                found      = true,
                userId     = parentUser.Id,
                parentName = $"{parentUser.FirstName} {parentUser.LastName}".Trim(),
                parentEmail = parentUser.Email
            });
        }

        // ── AJAX: search contacts ────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> SearchContacts(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return Json(Array.Empty<object>());

            var (currentUser, profile) = await GetCurrentUser();
            if (profile == null) return Unauthorized();

            var assignments   = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var myClassIds    = assignments.Where(ta => ta.TeacherId == profile.Id)
                .Select(ta => ta.ClassRoomId).Distinct().ToList();
            var studentProfiles = await _unitOfWork.StudentProfiles.GetAllAsync();
            var myStudentUserIds = studentProfiles
                .Where(sp => sp.ClassRoomId.HasValue && myClassIds.Contains(sp.ClassRoomId.Value))
                .Select(sp => sp.UserId).ToList();

            var query = q.ToLower();
            var allUsers = _userManager.Users.ToList();

            // Include my students + their parents
            var parentEmails = studentProfiles
                .Where(sp => myClassIds.Contains(sp.ClassRoomId ?? Guid.Empty) && !string.IsNullOrEmpty(sp.ParentEmail))
                .Select(sp => sp.ParentEmail!.ToLower()).ToHashSet();

            var contacts = allUsers
                .Where(u => u.Id != currentUser.Id && u.IsActive &&
                    (myStudentUserIds.Contains(u.Id) ||
                     (u.Email != null && parentEmails.Contains(u.Email.ToLower()))))
                .Where(u => $"{u.FirstName} {u.LastName}".ToLower().Contains(query) ||
                            (u.Email ?? "").ToLower().Contains(query))
                .Take(15)
                .ToList();

            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var result = new List<object>();

            foreach (var u in contacts)
            {
                var roles    = await _userManager.GetRolesAsync(u);
                var role     = roles.FirstOrDefault() ?? "";
                var sp2      = studentProfiles.FirstOrDefault(sp => sp.UserId == u.Id);
                var cn       = sp2?.ClassRoomId.HasValue == true
                    ? classRooms.FirstOrDefault(c => c.Id == sp2.ClassRoomId)?.Name ?? ""
                    : "";
                result.Add(new
                {
                    userId   = u.Id,
                    name     = $"{u.FirstName} {u.LastName}".Trim(),
                    role,
                    studentProfileId = sp2?.Id,
                    studentNumber    = sp2?.StudentNumber ?? "",
                    className        = cn
                });
            }
            return Json(result);
        }

        // ── Build student context (used internally) ──────────────────────────

        private async Task<StudentContextDto?> BuildStudentContext(Guid studentProfileId)
        {
            var sp = await _unitOfWork.StudentProfiles.GetByIdAsync(studentProfileId);
            if (sp == null) return null;

            var allUsers  = _userManager.Users.ToList();
            var user      = allUsers.FirstOrDefault(u => u.Id == sp.UserId);
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var cr         = classRooms.FirstOrDefault(c => c.Id == sp.ClassRoomId.GetValueOrDefault());

            var studentExams  = await _unitOfWork.StudentExams.GetAllAsync();
            var sExams        = studentExams.Where(se => se.StudentId == sp.Id && se.IsSubmitted).ToList();
            var avgScore      = sExams.Any() ? Math.Round(sExams.Average(e => e.Score), 1) : 0;

            var exams         = await _unitOfWork.Exams.GetAllAsync();
            var latestSE      = sExams.OrderByDescending(se =>
                exams.FirstOrDefault(e => e.Id == se.ExamId)?.ExamDate ?? DateTime.MinValue).FirstOrDefault();
            var latestExam    = latestSE != null ? exams.FirstOrDefault(e => e.Id == latestSE.ExamId) : null;

            var assignments   = await _unitOfWork.Assignments.GetAllAsync();
            var classAssignIds= assignments.Where(a => a.ClassRoomId == sp.ClassRoomId).Select(a => a.Id).ToList();
            var submissions   = await _unitOfWork.AssignmentSubmissions.GetAllAsync();
            var submitted     = submissions.Count(s => s.StudentId == sp.Id && classAssignIds.Contains(s.AssignmentId));
            var assignRate    = classAssignIds.Count > 0 ? Math.Round((double)submitted / classAssignIds.Count * 100, 1) : 0;
            var pending       = classAssignIds.Count - submitted;

            var attendanceSessions = await _unitOfWork.AttendanceSessions.GetAllAsync();
            var attendanceRecords  = await _unitOfWork.AttendanceRecords.GetAllAsync();
            var crSessions    = attendanceSessions.Where(s => s.ClassRoomId == sp.ClassRoomId).Select(s => s.Id).ToList();
            var sAttendance   = attendanceRecords.Where(r => r.StudentId == sp.Id && crSessions.Contains(r.AttendanceSessionId)).ToList();
            var presentCount  = sAttendance.Count(r => r.IsPresent);
            var attendRate    = sAttendance.Count > 0 ? Math.Round((double)presentCount / sAttendance.Count * 100, 1) : 100;
            var absenceCount  = sAttendance.Count(r => !r.IsPresent);

            // Performance level
            var score = (avgScore * 0.5) + (attendRate * 0.3) + (assignRate * 0.2);
            string level, color, emoji;
            if (score >= 80)       { level = "Excellent";         color = "#71dd37"; emoji = "🟢"; }
            else if (score >= 65)  { level = "Good";              color = "#696cff"; emoji = "🔵"; }
            else if (score >= 50)  { level = "Needs Improvement"; color = "#fd7e14"; emoji = "🟠"; }
            else                   { level = "At Risk";            color = "#e74c3c"; emoji = "🔴"; }

            var alerts = new List<string>();
            if (attendRate < 75)  alerts.Add($"⚠️ Attendance critically low ({attendRate}%)");
            if (avgScore < 50 && sExams.Any()) alerts.Add($"⚠️ Average exam grade below passing ({avgScore}%)");
            if (pending > 0)      alerts.Add($"📋 {pending} assignment(s) not submitted");
            if (absenceCount >= 5) alerts.Add($"📅 {absenceCount} absences recorded");

            return new StudentContextDto
            {
                ProfileId                = sp.Id,
                FullName                 = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown",
                StudentNumber            = sp.StudentNumber ?? "N/A",
                ClassName                = cr?.Name ?? "N/A",
                AttendanceRate           = attendRate,
                AbsenceCount             = absenceCount,
                LatestExamGrade          = latestSE?.Score ?? 0,
                LatestExamTotal          = latestExam?.TotalMarks ?? 0,
                LatestExamTitle          = latestExam?.Title ?? "—",
                AverageGrade             = avgScore,
                AssignmentCompletionRate = assignRate,
                PendingAssignments       = Math.Max(0, pending),
                PerformanceLevel         = level,
                PerformanceColor         = color,
                PerformanceEmoji         = emoji,
                Alerts                   = alerts
            };
        }
    }

    public class SendChatRequest
    {
        public Guid ReceiverId { get; set; }
        public string Body { get; set; } = string.Empty;
    }
}
