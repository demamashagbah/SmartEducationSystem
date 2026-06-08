using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Constants;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Web.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = Roles.Teacher)]
    public class ClassesController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClassesController(ITeacherService teacherService, IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
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

            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var myAssignments = teacherAssignments.Where(ta => ta.TeacherId == profile.Id).ToList();
            var myClassIds = myAssignments.Select(ta => ta.ClassRoomId).Distinct().ToList();

            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var grades     = await _unitOfWork.Grades.GetAllAsync();
            var subjects   = await _unitOfWork.Subjects.GetAllAsync();
            var studentProfiles = await _unitOfWork.StudentProfiles.GetAllAsync();
            var attendanceSessions = await _unitOfWork.AttendanceSessions.GetAllAsync();
            var attendanceRecords  = await _unitOfWork.AttendanceRecords.GetAllAsync();
            var assignments = await _unitOfWork.Assignments.GetAllAsync();
            var exams       = await _unitOfWork.Exams.GetAllAsync();

            var result = myClassIds.Select(classId =>
            {
                var cr    = classRooms.FirstOrDefault(c => c.Id == classId);
                if (cr == null) return null;
                var grade = grades.FirstOrDefault(g => g.Id == cr.GradeId);

                var classSubjectIds = myAssignments.Where(ta => ta.ClassRoomId == classId)
                    .Select(ta => ta.SubjectId).Distinct().ToList();
                var classSubjectNames = subjects.Where(s => classSubjectIds.Contains(s.Id))
                    .Select(s => s.Name).ToList();

                var classStudents = studentProfiles.Where(s => s.ClassRoomId == (Guid?)classId).ToList();
                var studentCount  = classStudents.Count;

                var classSessions = attendanceSessions.Where(s => s.ClassRoomId == classId).ToList();
                var sessionIds    = classSessions.Select(s => s.Id).ToList();
                var presentCount  = attendanceRecords.Count(r => sessionIds.Contains(r.AttendanceSessionId) && r.IsPresent);
                var totalRecords  = attendanceRecords.Count(r => sessionIds.Contains(r.AttendanceSessionId));
                var attendanceRate = totalRecords > 0 ? Math.Round((double)presentCount / totalRecords * 100, 1) : 0;

                var upcomingExams = exams.Count(e => classSubjectIds.Contains(e.SubjectId)
                    && e.ClassRoomId == classId && e.ExamDate > DateTime.UtcNow);
                var activeAssignments = assignments.Count(a => classSubjectIds.Contains(a.SubjectId)
                    && a.ClassRoomId == classId && a.DueDate > DateTime.UtcNow);

                return new TeacherClassViewModel
                {
                    ClassRoomId      = classId,
                    ClassName        = cr.Name,
                    GradeName        = grade?.Name ?? "N/A",
                    StudentCount     = studentCount,
                    Subjects         = classSubjectNames,
                    AttendanceRate   = attendanceRate,
                    TotalSessions    = classSessions.Count,
                    UpcomingExams    = upcomingExams,
                    ActiveAssignments = activeAssignments
                };
            }).Where(x => x != null).Cast<TeacherClassViewModel>().ToList();

            return View(result);
        }
    }

    public class TeacherClassViewModel
    {
        public Guid ClassRoomId { get; set; }
        public string ClassName { get; set; } = default!;
        public string GradeName { get; set; } = default!;
        public int StudentCount { get; set; }
        public List<string> Subjects { get; set; } = new();
        public double AttendanceRate { get; set; }
        public int TotalSessions { get; set; }
        public int UpcomingExams { get; set; }
        public int ActiveAssignments { get; set; }
    }
}
