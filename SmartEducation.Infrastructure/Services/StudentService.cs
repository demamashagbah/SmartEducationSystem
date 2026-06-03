using Microsoft.AspNetCore.Identity;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Infrastructure.Services
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<StudentProfile?> GetProfileByUserIdAsync(Guid userId)
        {
            var profiles = await _unitOfWork.StudentProfiles.GetAllAsync();
            return profiles.FirstOrDefault(s => s.UserId == userId);
        }

        public async Task<StudentDashboardDto> GetDashboardAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var profile = await GetProfileByUserIdAsync(userId);

            if (user == null || profile == null)
                return new StudentDashboardDto { StudentName = "Unknown" };

            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var grades = await _unitOfWork.Grades.GetAllAsync();
            var classRoom = classRooms.FirstOrDefault(c => c.Id == profile.ClassRoomId);
            var grade = classRoom != null ? grades.FirstOrDefault(g => g.Id == classRoom.GradeId) : null;

            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var classSubjectIds = teacherAssignments.Where(ta => ta.ClassRoomId == profile.ClassRoomId)
                                                    .Select(ta => ta.SubjectId).Distinct().ToList();
            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var mySubjects = subjects.Where(s => classSubjectIds.Contains(s.Id)).ToList();

            var assignments = await _unitOfWork.Assignments.GetAllAsync();
            var myAssignments = assignments.Where(a => a.ClassRoomId == profile.ClassRoomId).ToList();
            var submissions = await _unitOfWork.AssignmentSubmissions.GetAllAsync();
            var mySubmissions = submissions.Where(s => s.StudentId == profile.Id).ToList();
            var pendingAssignments = myAssignments.Count(a =>
                a.DueDate >= DateTime.UtcNow &&
                !mySubmissions.Any(s => s.AssignmentId == a.Id));

            var exams = await _unitOfWork.Exams.GetAllAsync();
            var classExams = exams.Where(e => e.ClassRoomId == profile.ClassRoomId || classSubjectIds.Contains(e.SubjectId)).ToList();
            var upcomingExams = classExams.Count(e => e.ExamDate > DateTime.UtcNow);

            var studentExams = await _unitOfWork.StudentExams.GetAllAsync();
            var myExams = studentExams.Where(se => se.StudentId == profile.Id && se.IsSubmitted).ToList();
            var avgScore = myExams.Any() ? myExams.Average(e => e.Score) : 0;
            var gpa = avgScore > 0 ? Math.Round(avgScore / 25.0, 2) : 0;

            var messages = await _unitOfWork.Messages.GetAllAsync();
            var unreadCount = messages.Count(m => m.ReceiverId == userId && !m.IsRead);

            // Subject summaries
            var subjectSummaries = new List<StudentSubjectSummaryDto>();
            foreach (var subject in mySubjects.Take(6))
            {
                var teacher = teacherAssignments.FirstOrDefault(ta =>
                    ta.SubjectId == subject.Id && ta.ClassRoomId == profile.ClassRoomId);
                var teacherProfiles = await _unitOfWork.TeacherProfiles.GetAllAsync();
                var teacherProfile = teacher != null ? teacherProfiles.FirstOrDefault(tp => tp.Id == teacher.TeacherId) : null;
                var teacherUser = teacherProfile != null
                    ? await _userManager.FindByIdAsync(teacherProfile.UserId.ToString())
                    : null;
                var units = await _unitOfWork.Units.GetAllAsync();
                var unitIds = units.Where(u => u.SubjectId == subject.Id).Select(u => u.Id).ToList();
                var lessons = await _unitOfWork.Lessons.GetAllAsync();
                var lessonCount = lessons.Count(l => unitIds.Contains(l.UnitId));

                var subjectExams = myExams.Where(se =>
                    classExams.Any(e => e.Id == se.ExamId && e.SubjectId == subject.Id)).ToList();
                var subjectAvg = subjectExams.Any() ? subjectExams.Average(e => e.Score) : 0;

                subjectSummaries.Add(new StudentSubjectSummaryDto
                {
                    SubjectId = subject.Id,
                    SubjectName = subject.Name,
                    AverageScore = Math.Round(subjectAvg, 1),
                    TotalLessons = lessonCount,
                    TeacherName = teacherUser != null ? $"{teacherUser.FirstName} {teacherUser.LastName}" : "N/A"
                });
            }

            // AI Recommendations
            var recommendations = GenerateAIRecommendations(myExams, myAssignments, mySubmissions, subjectSummaries);

            // Upcoming items
            var upcomingItems = new List<UpcomingItemDto>();
            foreach (var exam in classExams.Where(e => e.ExamDate > DateTime.UtcNow).Take(3))
            {
                var subject = subjects.FirstOrDefault(s => s.Id == exam.SubjectId);
                var daysUntil = (exam.ExamDate - DateTime.UtcNow).Days;
                upcomingItems.Add(new UpcomingItemDto
                {
                    Title = exam.Title,
                    Type = "Exam",
                    DueDate = exam.ExamDate,
                    SubjectName = subject?.Name ?? "",
                    UrgencyLevel = daysUntil <= 2 ? "Urgent" : daysUntil <= 7 ? "Normal" : "Upcoming"
                });
            }
            foreach (var assignment in myAssignments.Where(a => a.DueDate > DateTime.UtcNow).Take(3))
            {
                var subject = subjects.FirstOrDefault(s => s.Id == assignment.SubjectId);
                var daysUntil = (assignment.DueDate - DateTime.UtcNow).Days;
                upcomingItems.Add(new UpcomingItemDto
                {
                    Title = assignment.Title,
                    Type = "Assignment",
                    DueDate = assignment.DueDate,
                    SubjectName = subject?.Name ?? "",
                    UrgencyLevel = daysUntil <= 2 ? "Urgent" : daysUntil <= 7 ? "Normal" : "Upcoming"
                });
            }

            var weakTopics = subjectSummaries.Where(s => s.AverageScore > 0 && s.AverageScore < 60)
                                              .Select(s => s.SubjectName).ToList();
            var strongTopics = subjectSummaries.Where(s => s.AverageScore >= 80)
                                               .Select(s => s.SubjectName).ToList();

            return new StudentDashboardDto
            {
                StudentName = $"{user.FirstName} {user.LastName}",
                ClassName = classRoom?.Name ?? "N/A",
                GradeName = grade?.Name ?? "N/A",
                TotalSubjects = mySubjects.Count,
                PendingAssignments = pendingAssignments,
                UpcomingExams = upcomingExams,
                AttendanceRate = 92.0,
                GPA = gpa,
                CompletedTasks = mySubmissions.Count + myExams.Count,
                LearningProgress = mySubjects.Count > 0 ? Math.Min(100, (myExams.Count * 10.0)) : 0,
                UnreadMessages = unreadCount,
                Subjects = subjectSummaries,
                WeakTopics = weakTopics,
                StrongTopics = strongTopics,
                AIRecommendations = recommendations,
                UpcomingItems = upcomingItems.OrderBy(ui => ui.DueDate).Take(5).ToList()
            };
        }

        private List<AIRecommendationItemDto> GenerateAIRecommendations(
            IEnumerable<StudentExam> myExams,
            IEnumerable<Assignment> myAssignments,
            IEnumerable<AssignmentSubmission> mySubmissions,
            List<StudentSubjectSummaryDto> subjectSummaries)
        {
            var recommendations = new List<AIRecommendationItemDto>();

            foreach (var subject in subjectSummaries.Where(s => s.AverageScore > 0 && s.AverageScore < 65))
            {
                recommendations.Add(new AIRecommendationItemDto
                {
                    Category = "Performance",
                    Topic = subject.SubjectName,
                    Recommendation = $"Your performance in {subject.SubjectName} needs improvement. Focus on reviewing core concepts and practice more exercises.",
                    Priority = subject.AverageScore < 50 ? "High" : "Medium",
                    Icon = "bx-trending-down",
                    Color = subject.AverageScore < 50 ? "#ff3e1d" : "#fd7e14"
                });
            }

            if (!mySubmissions.Any() && myAssignments.Any())
            {
                recommendations.Add(new AIRecommendationItemDto
                {
                    Category = "Assignments",
                    Topic = "Assignment Completion",
                    Recommendation = "You have pending assignments. Completing assignments on time improves your overall grade significantly.",
                    Priority = "High",
                    Icon = "bx-task",
                    Color = "#ff3e1d"
                });
            }

            recommendations.Add(new AIRecommendationItemDto
            {
                Category = "Study Plan",
                Topic = "Daily Practice",
                Recommendation = "Establish a consistent daily study schedule of 2-3 hours. Regular practice improves retention by 40%.",
                Priority = "Medium",
                Icon = "bx-time",
                Color = "#696cff"
            });

            if (!recommendations.Any(r => r.Category == "Performance"))
            {
                recommendations.Add(new AIRecommendationItemDto
                {
                    Category = "Performance",
                    Topic = "Excellent Progress",
                    Recommendation = "You are performing well! Keep up the great work and challenge yourself with advanced topics.",
                    Priority = "Low",
                    Icon = "bx-star",
                    Color = "#71dd37"
                });
            }

            return recommendations.Take(5).ToList();
        }

        public async Task<IEnumerable<StudentSubjectSummaryDto>> GetSubjectsAsync(Guid studentProfileId)
        {
            var profile = await _unitOfWork.StudentProfiles.GetByIdAsync(studentProfileId);
            if (profile == null) return Enumerable.Empty<StudentSubjectSummaryDto>();

            var dashboard = await GetDashboardAsync(profile.UserId);
            return dashboard.Subjects;
        }

        public async Task<IEnumerable<AssignmentDto>> GetAssignmentsAsync(Guid studentProfileId)
        {
            var profile = await _unitOfWork.StudentProfiles.GetByIdAsync(studentProfileId);
            if (profile == null) return Enumerable.Empty<AssignmentDto>();

            var assignments = await _unitOfWork.Assignments.GetAllAsync();
            var myAssignments = assignments.Where(a => a.ClassRoomId == profile.ClassRoomId).ToList();
            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var submissions = await _unitOfWork.AssignmentSubmissions.GetAllAsync();

            return myAssignments.Select(a =>
            {
                var submission = submissions.FirstOrDefault(s => s.AssignmentId == a.Id && s.StudentId == studentProfileId);
                return new AssignmentDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    SubjectId = a.SubjectId,
                    SubjectName = subjects.FirstOrDefault(s => s.Id == a.SubjectId)?.Name ?? "",
                    ClassRoomId = a.ClassRoomId,
                    DueDate = a.DueDate,
                    MaxScore = a.MaxScore,
                    SubmissionCount = submission != null ? 1 : 0
                };
            }).OrderBy(a => a.DueDate).ToList();
        }

        public async Task<bool> SubmitAssignmentAsync(Guid assignmentId, Guid studentProfileId, string? notes)
        {
            var submissions = await _unitOfWork.AssignmentSubmissions.GetAllAsync();
            if (submissions.Any(s => s.AssignmentId == assignmentId && s.StudentId == studentProfileId))
                return false;

            await _unitOfWork.AssignmentSubmissions.AddAsync(new AssignmentSubmission
            {
                Id = Guid.NewGuid(),
                AssignmentId = assignmentId,
                StudentId = studentProfileId,
                Notes = notes,
                SubmittedAt = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ExamDto>> GetUpcomingExamsAsync(Guid studentProfileId)
        {
            var profile = await _unitOfWork.StudentProfiles.GetByIdAsync(studentProfileId);
            if (profile == null) return Enumerable.Empty<ExamDto>();

            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var classSubjectIds = teacherAssignments.Where(ta => ta.ClassRoomId == profile.ClassRoomId)
                                                    .Select(ta => ta.SubjectId).Distinct().ToList();
            var exams = await _unitOfWork.Exams.GetAllAsync();
            var upcomingExams = exams.Where(e =>
                (e.ClassRoomId == profile.ClassRoomId || classSubjectIds.Contains(e.SubjectId)) &&
                e.ExamDate > DateTime.UtcNow).ToList();

            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            return upcomingExams.Select(e => new ExamDto
            {
                Id = e.Id,
                Title = e.Title,
                SubjectId = e.SubjectId,
                SubjectName = subjects.FirstOrDefault(s => s.Id == e.SubjectId)?.Name ?? "",
                ExamDate = e.ExamDate,
                TotalMarks = e.TotalMarks,
                DurationMinutes = e.DurationMinutes,
                ExamType = e.ExamType
            }).OrderBy(e => e.ExamDate).ToList();
        }
    }
}
