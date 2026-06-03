using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartEducation.Application.Constants;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Web.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = Roles.Teacher)]
    public class AttendanceController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public AttendanceController(ITeacherService teacherService, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _teacherService = teacherService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        private async Task<TeacherProfile?> GetTeacherProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            return user != null ? await _teacherService.GetProfileByUserIdAsync(user.Id) : null;
        }

        public async Task<IActionResult> Index()
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var sessions = await _teacherService.GetAttendanceSessionsAsync(profile.Id);
            return View(sessions);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            await PopulateDropdowns(profile.Id);
            ViewBag.SessionDate = DateTime.Today.ToString("yyyy-MM-dd");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid classRoomId, Guid subjectId, DateTime sessionDate)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var session = await _teacherService.CreateAttendanceSessionAsync(profile.Id, classRoomId, subjectId, sessionDate);
            TempData["Success"] = "Attendance session created. Mark student attendance below.";
            return RedirectToAction(nameof(MarkAttendance), new { id = session.Id });
        }

        [HttpGet]
        public async Task<IActionResult> MarkAttendance(Guid id)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var sessions = await _unitOfWork.AttendanceSessions.GetAllAsync();
            var session = sessions.FirstOrDefault(s => s.Id == id && s.TeacherId == profile.Id);
            if (session == null) return NotFound();

            var studentProfiles = await _unitOfWork.StudentProfiles.GetAllAsync();
            var classStudents = studentProfiles.Where(s => s.ClassRoomId == session.ClassRoomId).ToList();
            var allUsers = _userManager.Users.ToList();

            var records = await _unitOfWork.AttendanceRecords.GetAllAsync();
            var sessionRecords = records.Where(r => r.AttendanceSessionId == id).ToList();

            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();

            ViewBag.Session = session;
            ViewBag.SubjectName = subjects.FirstOrDefault(s => s.Id == session.SubjectId)?.Name ?? "";
            ViewBag.ClassName = classRooms.FirstOrDefault(c => c.Id == session.ClassRoomId)?.Name ?? "";

            var studentAttendanceList = classStudents.Select(sp =>
            {
                var user = allUsers.FirstOrDefault(u => u.Id == sp.UserId);
                var record = sessionRecords.FirstOrDefault(r => r.StudentId == sp.Id);
                return new
                {
                    StudentProfileId = sp.Id,
                    StudentName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                    IsPresent = record?.IsPresent ?? true
                };
            }).ToList();

            ViewBag.Students = studentAttendanceList;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAttendance(Guid sessionId, Dictionary<string, string> attendance)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var statusDict = attendance
                .Where(kv => kv.Key.StartsWith("status_"))
                .ToDictionary(
                    kv => Guid.Parse(kv.Key.Replace("status_", "")),
                    kv => kv.Value
                );

            if (statusDict.Any())
            {
                await _teacherService.MarkAttendanceWithStatusAsync(sessionId, statusDict);
            }
            else
            {
                // Legacy fallback: boolean present/absent
                var attendanceDict = attendance
                    .Where(kv => kv.Key.StartsWith("attendance_"))
                    .ToDictionary(
                        kv => Guid.Parse(kv.Key.Replace("attendance_", "")),
                        kv => kv.Value == "true"
                    );
                await _teacherService.MarkAttendanceAsync(sessionId, attendanceDict);
            }

            TempData["Success"] = "Attendance saved successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(Guid teacherProfileId)
        {
            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var myAssignments = teacherAssignments.Where(ta => ta.TeacherId == teacherProfileId).ToList();
            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();

            var myClassIds = myAssignments.Select(ta => ta.ClassRoomId).Distinct().ToList();
            var mySubjectIds = myAssignments.Select(ta => ta.SubjectId).Distinct().ToList();

            ViewBag.ClassRooms = classRooms.Where(c => myClassIds.Contains(c.Id))
                .Select(c => new SelectListItem(c.Name, c.Id.ToString()));
            ViewBag.Subjects = subjects.Where(s => mySubjectIds.Contains(s.Id))
                .Select(s => new SelectListItem(s.Name, s.Id.ToString()));
        }
    }
}
