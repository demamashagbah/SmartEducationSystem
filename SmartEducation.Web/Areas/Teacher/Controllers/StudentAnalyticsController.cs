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
    public class StudentAnalyticsController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentAnalyticsController(ITeacherService teacherService, IUnitOfWork unitOfWork,
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

            var students = await BuildStudentAnalyticsList(profile.Id);
            return View(students);
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var detail = await BuildStudentDetail(id, profile.Id);
            if (detail == null) return NotFound();
            return View(detail);
        }

        private async Task<List<StudentAnalyticsViewModel>> BuildStudentAnalyticsList(Guid teacherProfileId)
        {
            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var myClassIds = teacherAssignments.Where(ta => ta.TeacherId == teacherProfileId)
                .Select(ta => ta.ClassRoomId).Distinct().ToList();

            var studentProfiles = await _unitOfWork.StudentProfiles.GetAllAsync();
            var myStudents = studentProfiles
                .Where(s => s.ClassRoomId.HasValue && myClassIds.Contains(s.ClassRoomId.Value))
                .ToList();

            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var allUsers = _userManager.Users.ToList();
            var studentExams = await _unitOfWork.StudentExams.GetAllAsync();
            var submissions = await _unitOfWork.AssignmentSubmissions.GetAllAsync();
            var assignments = await _unitOfWork.Assignments.GetAllAsync();
            var attendanceRecords = await _unitOfWork.AttendanceRecords.GetAllAsync();
            var attendanceSessions = await _unitOfWork.AttendanceSessions.GetAllAsync();
            var mySessionIds = attendanceSessions.Where(s => myClassIds.Contains(s.ClassRoomId))
                .Select(s => s.Id).ToList();

            var result = new List<StudentAnalyticsViewModel>();

            foreach (var sp in myStudents)
            {
                var user = allUsers.FirstOrDefault(u => u.Id == sp.UserId);
                var cr = classRooms.FirstOrDefault(c => c.Id == sp.ClassRoomId.GetValueOrDefault());

                var sExams = studentExams.Where(se => se.StudentId == sp.Id && se.IsSubmitted).ToList();
                var avgScore = sExams.Any() ? Math.Round(sExams.Average(e => e.Score), 1) : 0;

                var classAssignmentIds = assignments.Where(a => a.ClassRoomId == sp.ClassRoomId).Select(a => a.Id).ToList();
                var studentSubmissions = submissions.Where(s => s.StudentId == sp.Id && classAssignmentIds.Contains(s.AssignmentId)).ToList();
                var assignmentRate = classAssignmentIds.Count > 0
                    ? Math.Round((studentSubmissions.Count / (double)classAssignmentIds.Count) * 100, 1)
                    : 0;

                var studentAttendance = attendanceRecords.Where(r => r.StudentId == sp.Id && mySessionIds.Contains(r.AttendanceSessionId)).ToList();
                var attendanceRate = studentAttendance.Count > 0
                    ? Math.Round(studentAttendance.Count(r => r.IsPresent) / (double)studentAttendance.Count * 100, 1)
                    : 100;

                var riskLevel = avgScore > 0 && avgScore < 50 ? "High"
                    : (avgScore < 70 || attendanceRate < 75) ? "Medium"
                    : "Low";

                result.Add(new StudentAnalyticsViewModel
                {
                    StudentProfileId = sp.Id,
                    StudentName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                    ClassName = cr?.Name ?? "N/A",
                    AverageScore = avgScore,
                    AttendanceRate = attendanceRate,
                    AssignmentCompletionRate = assignmentRate,
                    ExamsTaken = sExams.Count,
                    RiskLevel = riskLevel
                });
            }

            return result.OrderBy(s => s.StudentName).ToList();
        }

        private async Task<StudentAnalyticsDetailViewModel?> BuildStudentDetail(Guid studentProfileId, Guid teacherProfileId)
        {
            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var myClassIds = teacherAssignments.Where(ta => ta.TeacherId == teacherProfileId)
                .Select(ta => ta.ClassRoomId).Distinct().ToList();

            var sp = await _unitOfWork.StudentProfiles.GetByIdAsync(studentProfileId);
            if (sp == null || !sp.ClassRoomId.HasValue || !myClassIds.Contains(sp.ClassRoomId.Value))
                return null;

            var allUsers = _userManager.Users.ToList();
            var user = allUsers.FirstOrDefault(u => u.Id == sp.UserId);
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var cr = classRooms.FirstOrDefault(c => c.Id == sp.ClassRoomId.GetValueOrDefault());

            var studentExams = await _unitOfWork.StudentExams.GetAllAsync();
            var sExams = studentExams.Where(se => se.StudentId == sp.Id && se.IsSubmitted).ToList();
            var avgScore = sExams.Any() ? Math.Round(sExams.Average(e => e.Score), 1) : 0;

            var exams = await _unitOfWork.Exams.GetAllAsync();
            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var assignments = await _unitOfWork.Assignments.GetAllAsync();
            var submissions = await _unitOfWork.AssignmentSubmissions.GetAllAsync();
            var classAssignmentIds = assignments.Where(a => a.ClassRoomId == sp.ClassRoomId).Select(a => a.Id).ToList();
            var studentSubmissions = submissions.Where(s => s.StudentId == sp.Id && classAssignmentIds.Contains(s.AssignmentId)).ToList();
            var assignmentRate = classAssignmentIds.Count > 0
                ? Math.Round((studentSubmissions.Count / (double)classAssignmentIds.Count) * 100, 1) : 0;

            var attendanceSessions = await _unitOfWork.AttendanceSessions.GetAllAsync();
            var attendanceRecords = await _unitOfWork.AttendanceRecords.GetAllAsync();
            var mySessionIds = attendanceSessions.Where(s => myClassIds.Contains(s.ClassRoomId)).Select(s => s.Id).ToList();
            var studentAttendance = attendanceRecords.Where(r => r.StudentId == sp.Id && mySessionIds.Contains(r.AttendanceSessionId)).ToList();
            var attendanceRate = studentAttendance.Count > 0
                ? Math.Round(studentAttendance.Count(r => r.IsPresent) / (double)studentAttendance.Count * 100, 1) : 100;

            var examHistory = sExams.Select(se =>
            {
                var exam = exams.FirstOrDefault(e => e.Id == se.ExamId);
                return new ExamResultEntry
                {
                    ExamTitle = exam?.Title ?? "Unknown",
                    Score = se.Score,
                    TotalMarks = exam?.TotalMarks ?? 0,
                    Date = exam?.ExamDate ?? DateTime.MinValue
                };
            }).OrderByDescending(e => e.Date).Take(10).ToList();

            var subjectScores = new List<SubjectScoreEntry>();
            var mySubjectIds = teacherAssignments.Where(ta => ta.TeacherId == teacherProfileId)
                .Select(ta => ta.SubjectId).Distinct().ToList();
            foreach (var subjectId in mySubjectIds)
            {
                var subject = subjects.FirstOrDefault(s => s.Id == subjectId);
                if (subject == null) continue;
                var subjectExamIds = exams.Where(e => e.SubjectId == subjectId).Select(e => e.Id).ToList();
                var subjectStudentExams = sExams.Where(se => subjectExamIds.Contains(se.ExamId)).ToList();
                if (subjectStudentExams.Any())
                {
                    subjectScores.Add(new SubjectScoreEntry
                    {
                        SubjectName = subject.Name,
                        AverageScore = Math.Round(subjectStudentExams.Average(se => se.Score), 1)
                    });
                }
            }

            // AI-style insights based on data patterns
            var insights = GenerateInsights(avgScore, attendanceRate, assignmentRate, subjectScores);

            var riskLevel = avgScore > 0 && avgScore < 50 ? "High"
                : (avgScore < 70 || attendanceRate < 75) ? "Medium" : "Low";

            return new StudentAnalyticsDetailViewModel
            {
                StudentProfileId = sp.Id,
                StudentName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                StudentNumber = sp.StudentNumber ?? "N/A",
                ClassName = cr?.Name ?? "N/A",
                AverageScore = avgScore,
                AttendanceRate = attendanceRate,
                AssignmentCompletionRate = assignmentRate,
                ExamsTaken = sExams.Count,
                RiskLevel = riskLevel,
                ExamHistory = examHistory,
                SubjectScores = subjectScores,
                Insights = insights
            };
        }

        private static List<string> GenerateInsights(double avgScore, double attendanceRate, double assignmentRate, List<SubjectScoreEntry> subjectScores)
        {
            var insights = new List<string>();

            if (attendanceRate < 75)
                insights.Add("Attendance is critically low. Frequent absences are directly impacting academic performance. Immediate intervention recommended.");
            else if (attendanceRate < 85)
                insights.Add("Attendance is below the recommended 85% threshold. Consider a parent/guardian meeting to address this.");

            if (avgScore < 50)
                insights.Add("Student is performing below the passing threshold. Targeted remedial sessions and additional practice materials are strongly recommended.");
            else if (avgScore < 65)
                insights.Add("Student performance is below average. Consider additional support sessions focused on core concepts.");
            else if (avgScore >= 85)
                insights.Add("Student is performing excellently. Consider providing advanced extension activities to maintain engagement.");

            if (assignmentRate < 60)
                insights.Add("Low assignment completion rate detected. This indicates possible disengagement or workload issues. Consider a one-on-one check-in.");
            else if (assignmentRate >= 90)
                insights.Add("Excellent assignment completion rate. This student demonstrates strong work ethic and commitment.");

            var weakSubject = subjectScores.OrderBy(s => s.AverageScore).FirstOrDefault();
            var strongSubject = subjectScores.OrderByDescending(s => s.AverageScore).FirstOrDefault();
            if (weakSubject != null && weakSubject.AverageScore < 60)
                insights.Add($"Weakest subject: {weakSubject.SubjectName} ({weakSubject.AverageScore}%). Additional resources and practice exercises for this subject are recommended.");
            if (strongSubject != null && strongSubject.AverageScore >= 80 && strongSubject != weakSubject)
                insights.Add($"Strongest subject: {strongSubject.SubjectName} ({strongSubject.AverageScore}%). Consider using this subject's engagement strategies in weaker areas.");

            if (!insights.Any())
                insights.Add("Student performance is on track. Continue monitoring progress and providing regular feedback.");

            return insights;
        }
    }

    public class StudentAnalyticsViewModel
    {
        public Guid StudentProfileId { get; set; }
        public string StudentName { get; set; } = default!;
        public string ClassName { get; set; } = default!;
        public double AverageScore { get; set; }
        public double AttendanceRate { get; set; }
        public double AssignmentCompletionRate { get; set; }
        public int ExamsTaken { get; set; }
        public string RiskLevel { get; set; } = "Low";
    }

    public class StudentAnalyticsDetailViewModel
    {
        public Guid StudentProfileId { get; set; }
        public string StudentName { get; set; } = default!;
        public string StudentNumber { get; set; } = default!;
        public string ClassName { get; set; } = default!;
        public double AverageScore { get; set; }
        public double AttendanceRate { get; set; }
        public double AssignmentCompletionRate { get; set; }
        public int ExamsTaken { get; set; }
        public string RiskLevel { get; set; } = "Low";
        public List<ExamResultEntry> ExamHistory { get; set; } = new();
        public List<SubjectScoreEntry> SubjectScores { get; set; } = new();
        public List<string> Insights { get; set; } = new();
    }

    public class ExamResultEntry
    {
        public string ExamTitle { get; set; } = default!;
        public double Score { get; set; }
        public int TotalMarks { get; set; }
        public DateTime Date { get; set; }
    }

    public class SubjectScoreEntry
    {
        public string SubjectName { get; set; } = default!;
        public double AverageScore { get; set; }
    }
}
