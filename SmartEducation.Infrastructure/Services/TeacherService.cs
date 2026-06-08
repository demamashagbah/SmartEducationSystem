using Microsoft.AspNetCore.Identity;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Infrastructure.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeacherService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<TeacherProfile?> GetProfileByUserIdAsync(Guid userId)
        {
            var profiles = await _unitOfWork.TeacherProfiles.GetAllAsync();
            return profiles.FirstOrDefault(t => t.UserId == userId);
        }

        public async Task<TeacherDashboardDto> GetDashboardAsync(Guid userId)
        {
            var profile = await GetProfileByUserIdAsync(userId);
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (profile == null || user == null)
                return new TeacherDashboardDto { TeacherName = "Unknown" };

            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var myAssignments = teacherAssignments.Where(ta => ta.TeacherId == profile.Id).ToList();
            var mySubjectIds = myAssignments.Select(ta => ta.SubjectId).Distinct().ToList();
            var myClassIds = myAssignments.Select(ta => ta.ClassRoomId).Distinct().ToList();

            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var grades = await _unitOfWork.Grades.GetAllAsync();
            var studentProfiles = await _unitOfWork.StudentProfiles.GetAllAsync();
            var myStudents = studentProfiles.Where(s => s.ClassRoomId.HasValue && myClassIds.Contains(s.ClassRoomId.Value)).ToList();

            var lessonPlans = await _unitOfWork.LessonPlans.GetAllAsync();
            var myLessonPlanIds = myAssignments.Select(ta => ta.Id).ToList();
            var myLessonPlans = lessonPlans.Where(lp => myLessonPlanIds.Contains(lp.TeacherAssignmentId)).ToList();

            var units = await _unitOfWork.Units.GetAllAsync();
            var lessons = await _unitOfWork.Lessons.GetAllAsync();
            var assignments = await _unitOfWork.Assignments.GetAllAsync();
            var myAssignmentItems = assignments.Where(a => a.TeacherId == profile.Id).ToList();
            var exams = await _unitOfWork.Exams.GetAllAsync();
            var myExams = exams.Where(e => mySubjectIds.Contains(e.SubjectId)).ToList();
            var upcomingExams = myExams.Count(e => e.ExamDate > DateTime.UtcNow);
            var messages = await _unitOfWork.Messages.GetAllAsync();
            var unreadCount = messages.Count(m => m.ReceiverId == userId && !m.IsRead);

            // Subject coverage
            var subjectCoverage = new List<TeacherSubjectCoverageDto>();
            foreach (var subjectId in mySubjectIds)
            {
                var subject = subjects.FirstOrDefault(s => s.Id == subjectId);
                if (subject == null) continue;
                var subjectUnits = units.Where(u => u.SubjectId == subjectId).ToList();
                var unitIds = subjectUnits.Select(u => u.Id).ToList();
                var subjectLessons = lessons.Where(l => unitIds.Contains(l.UnitId)).ToList();
                var plannedLessons = myLessonPlans.Count(lp => lp.LessonId.HasValue &&
                    subjectLessons.Any(sl => sl.Id == lp.LessonId));
                var coverage = subjectLessons.Count > 0
                    ? (plannedLessons / (double)subjectLessons.Count) * 100
                    : 0;

                subjectCoverage.Add(new TeacherSubjectCoverageDto
                {
                    SubjectId = subjectId,
                    SubjectName = subject.Name,
                    TotalLessons = subjectLessons.Count,
                    PlannedLessons = plannedLessons,
                    CoveragePercent = Math.Round(coverage, 1)
                });
            }

            var overallCoverage = subjectCoverage.Any()
                ? subjectCoverage.Average(sc => sc.CoveragePercent)
                : 0;

            // Class summaries
            var classSummaries = new List<TeacherClassSummaryDto>();
            foreach (var classId in myClassIds)
            {
                var classRoom = classRooms.FirstOrDefault(c => c.Id == classId);
                if (classRoom == null) continue;
                var grade = grades.FirstOrDefault(g => g.Id == classRoom.GradeId);
                var classStudents = myStudents.Where(s => s.ClassRoomId == (Guid?)classId).ToList();
                var classSubjects = myAssignments.Where(ta => ta.ClassRoomId == classId)
                    .Select(ta => ta.SubjectId).Distinct().ToList();
                var classSubjectNames = subjects.Where(s => classSubjects.Contains(s.Id))
                    .Select(s => s.Name).ToList();

                classSummaries.Add(new TeacherClassSummaryDto
                {
                    ClassRoomId = classId,
                    ClassName = classRoom.Name,
                    GradeName = grade?.Name ?? "N/A",
                    StudentCount = classStudents.Count,
                    Subjects = classSubjectNames,
                    AveragePerformance = 75.0,
                    AttendanceRate = 90.0
                });
            }

            // Student summaries
            var allUsers = _userManager.Users.ToList();
            var studentSummaries = myStudents.Take(5).Select(s =>
            {
                var sUser = allUsers.FirstOrDefault(u => u.Id == s.UserId);
                var cr = classRooms.FirstOrDefault(c => c.Id == s.ClassRoomId.GetValueOrDefault());
                return new StudentSummaryDto
                {
                    StudentProfileId = s.Id,
                    StudentName = sUser != null ? $"{sUser.FirstName} {sUser.LastName}" : "Unknown",
                    ClassName = cr?.Name ?? "N/A",
                    AverageScore = 72.0,
                    AttendanceRate = 88.0,
                    RiskLevel = "Low"
                };
            }).ToList();

            return new TeacherDashboardDto
            {
                TeacherName = $"{user.FirstName} {user.LastName}",
                EmployeeNumber = profile.EmployeeNumber ?? "N/A",
                AssignedClasses = myClassIds.Count,
                AssignedSubjects = mySubjectIds.Count,
                TotalStudents = myStudents.Count,
                LessonPlansCount = myLessonPlans.Count,
                UpcomingExams = upcomingExams,
                PendingAssignments = myAssignmentItems.Count(a => a.DueDate > DateTime.UtcNow),
                AttendanceRate = 90.0,
                CurriculumCoverage = Math.Round(overallCoverage, 1),
                LearningOutcomeMastery = 75.0,
                AtRiskStudents = 0,
                UnreadMessages = unreadCount,
                Classes = classSummaries,
                SubjectCoverage = subjectCoverage,
                RecentStudentActivity = studentSummaries
            };
        }

        // ========== LESSON PLANS ==========

        public async Task<IEnumerable<LessonPlanDto>> GetLessonPlansAsync(Guid teacherProfileId)
        {
            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var myAssignmentIds = teacherAssignments.Where(ta => ta.TeacherId == teacherProfileId)
                                                    .Select(ta => ta.Id).ToList();
            var lessonPlans = await _unitOfWork.LessonPlans.GetAllAsync();
            var myPlans = lessonPlans.Where(lp => myAssignmentIds.Contains(lp.TeacherAssignmentId)).ToList();

            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var lessons = await _unitOfWork.Lessons.GetAllAsync();

            return myPlans.Select(lp =>
            {
                var ta = teacherAssignments.FirstOrDefault(t => t.Id == lp.TeacherAssignmentId);
                var subject = ta != null ? subjects.FirstOrDefault(s => s.Id == ta.SubjectId) : null;
                var classRoom = ta != null ? classRooms.FirstOrDefault(c => c.Id == ta.ClassRoomId) : null;
                var lesson = lp.LessonId.HasValue ? lessons.FirstOrDefault(l => l.Id == lp.LessonId) : null;

                return new LessonPlanDto
                {
                    Id = lp.Id,
                    TeacherAssignmentId = lp.TeacherAssignmentId,
                    LessonId = lp.LessonId,
                    LessonName = lesson?.Name,
                    Title = lp.Title,
                    LessonDate = lp.LessonDate,
                    Objectives = lp.Objectives,
                    Introduction = lp.Introduction,
                    Activities = lp.Activities,
                    TeachingStrategies = lp.TeachingStrategies,
                    AssessmentMethods = lp.AssessmentMethods,
                    Homework = lp.Homework,
                    DurationMinutes = lp.DurationMinutes,
                    IsAiGenerated = lp.IsAiGenerated,
                    SubjectName = subject?.Name ?? "",
                    ClassRoomName = classRoom?.Name ?? "",
                    SubjectId = subject?.Id ?? Guid.Empty,
                    ClassRoomId = classRoom?.Id ?? Guid.Empty
                };
            }).OrderByDescending(lp => lp.LessonDate).ToList();
        }

        public async Task<LessonPlanDto?> GetLessonPlanByIdAsync(Guid id, Guid teacherProfileId)
        {
            var lp = await _unitOfWork.LessonPlans.GetByIdAsync(id);
            if (lp == null) return null;

            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var ta = teacherAssignments.FirstOrDefault(t => t.Id == lp.TeacherAssignmentId);
            if (ta == null || ta.TeacherId != teacherProfileId) return null;

            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var lessons = await _unitOfWork.Lessons.GetAllAsync();
            var subject = subjects.FirstOrDefault(s => s.Id == ta.SubjectId);
            var classRoom = classRooms.FirstOrDefault(c => c.Id == ta.ClassRoomId);
            var lesson = lp.LessonId.HasValue ? lessons.FirstOrDefault(l => l.Id == lp.LessonId) : null;

            return new LessonPlanDto
            {
                Id = lp.Id,
                TeacherAssignmentId = lp.TeacherAssignmentId,
                LessonId = lp.LessonId,
                LessonName = lesson?.Name,
                Title = lp.Title,
                LessonDate = lp.LessonDate,
                Objectives = lp.Objectives,
                Introduction = lp.Introduction,
                Activities = lp.Activities,
                TeachingStrategies = lp.TeachingStrategies,
                AssessmentMethods = lp.AssessmentMethods,
                Homework = lp.Homework,
                DurationMinutes = lp.DurationMinutes,
                IsAiGenerated = lp.IsAiGenerated,
                SubjectName = subject?.Name ?? "",
                ClassRoomName = classRoom?.Name ?? "",
                SubjectId = subject?.Id ?? Guid.Empty,
                ClassRoomId = classRoom?.Id ?? Guid.Empty
            };
        }

        public async Task<LessonPlanDto> CreateLessonPlanAsync(LessonPlanDto dto)
        {
            var entity = new LessonPlan
            {
                Id = Guid.NewGuid(),
                TeacherAssignmentId = dto.TeacherAssignmentId,
                LessonId = dto.LessonId,
                Title = dto.Title,
                LessonDate = dto.LessonDate,
                Objectives = dto.Objectives,
                Introduction = dto.Introduction,
                Activities = dto.Activities,
                TeachingStrategies = dto.TeachingStrategies,
                AssessmentMethods = dto.AssessmentMethods,
                Homework = dto.Homework,
                DurationMinutes = dto.DurationMinutes,
                IsAiGenerated = dto.IsAiGenerated
            };
            await _unitOfWork.LessonPlans.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> UpdateLessonPlanAsync(LessonPlanDto dto, Guid teacherProfileId)
        {
            var lp = await _unitOfWork.LessonPlans.GetByIdAsync(dto.Id);
            if (lp == null) return false;
            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var ta = teacherAssignments.FirstOrDefault(t => t.Id == lp.TeacherAssignmentId);
            if (ta == null || ta.TeacherId != teacherProfileId) return false;

            lp.Title = dto.Title;
            lp.LessonDate = dto.LessonDate;
            lp.LessonId = dto.LessonId;
            lp.Objectives = dto.Objectives;
            lp.Introduction = dto.Introduction;
            lp.Activities = dto.Activities;
            lp.TeachingStrategies = dto.TeachingStrategies;
            lp.AssessmentMethods = dto.AssessmentMethods;
            lp.Homework = dto.Homework;
            lp.DurationMinutes = dto.DurationMinutes;
            await _unitOfWork.LessonPlans.UpdateAsync(lp);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteLessonPlanAsync(Guid id, Guid teacherProfileId)
        {
            var lp = await _unitOfWork.LessonPlans.GetByIdAsync(id);
            if (lp == null) return false;
            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var ta = teacherAssignments.FirstOrDefault(t => t.Id == lp.TeacherAssignmentId);
            if (ta == null || ta.TeacherId != teacherProfileId) return false;
            await _unitOfWork.LessonPlans.DeleteAsync(lp);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // ========== ATTENDANCE ==========

        public async Task<IEnumerable<AttendanceSessionDto>> GetAttendanceSessionsAsync(Guid teacherProfileId)
        {
            var sessions = await _unitOfWork.AttendanceSessions.GetAllAsync();
            var mySessions = sessions.Where(s => s.TeacherId == teacherProfileId)
                                     .OrderByDescending(s => s.SessionDate).ToList();
            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var records = await _unitOfWork.AttendanceRecords.GetAllAsync();

            return mySessions.Select(s =>
            {
                var sessionRecords = records.Where(r => r.AttendanceSessionId == s.Id).ToList();
                return new AttendanceSessionDto
                {
                    Id = s.Id,
                    ClassRoomId = s.ClassRoomId,
                    ClassRoomName = classRooms.FirstOrDefault(c => c.Id == s.ClassRoomId)?.Name ?? "",
                    SubjectId = s.SubjectId,
                    SubjectName = subjects.FirstOrDefault(sub => sub.Id == s.SubjectId)?.Name ?? "",
                    SessionDate = s.SessionDate,
                    TotalStudents = sessionRecords.Count,
                    PresentCount = sessionRecords.Count(r => r.IsPresent),
                    AbsentCount = sessionRecords.Count(r => !r.IsPresent)
                };
            });
        }

        public async Task<AttendanceSessionDto> CreateAttendanceSessionAsync(Guid teacherProfileId, Guid classRoomId, Guid subjectId, DateTime date)
        {
            var session = new AttendanceSession
            {
                Id = Guid.NewGuid(),
                ClassRoomId = classRoomId,
                SubjectId = subjectId,
                TeacherId = teacherProfileId,
                SessionDate = date
            };
            await _unitOfWork.AttendanceSessions.AddAsync(session);
            await _unitOfWork.SaveChangesAsync();

            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();

            return new AttendanceSessionDto
            {
                Id = session.Id,
                ClassRoomId = classRoomId,
                ClassRoomName = classRooms.FirstOrDefault(c => c.Id == classRoomId)?.Name ?? "",
                SubjectId = subjectId,
                SubjectName = subjects.FirstOrDefault(s => s.Id == subjectId)?.Name ?? "",
                SessionDate = date
            };
        }

        public async Task MarkAttendanceAsync(Guid sessionId, Dictionary<Guid, bool> studentAttendance)
        {
            var session = await _unitOfWork.AttendanceSessions.GetByIdAsync(sessionId);
            if (session == null) return;

            var existingRecords = await _unitOfWork.AttendanceRecords.GetAllAsync();
            var sessionRecords = existingRecords.Where(r => r.AttendanceSessionId == sessionId).ToList();

            foreach (var entry in studentAttendance)
            {
                var existing = sessionRecords.FirstOrDefault(r => r.StudentId == entry.Key);
                if (existing != null)
                {
                    existing.IsPresent = entry.Value;
                    await _unitOfWork.AttendanceRecords.UpdateAsync(existing);
                }
                else
                {
                    await _unitOfWork.AttendanceRecords.AddAsync(new AttendanceRecord
                    {
                        Id = Guid.NewGuid(),
                        AttendanceSessionId = sessionId,
                        StudentId = entry.Key,
                        IsPresent = entry.Value,
                        Status = entry.Value ? Domain.Enums.AttendanceStatus.Present : Domain.Enums.AttendanceStatus.Absent
                    });
                }
            }
            await _unitOfWork.SaveChangesAsync();
        }

        // ========== STUDENTS ==========

        public async Task<IEnumerable<StudentSummaryDto>> GetMyStudentsAsync(Guid teacherProfileId)
        {
            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var myClassIds = teacherAssignments.Where(ta => ta.TeacherId == teacherProfileId)
                                               .Select(ta => ta.ClassRoomId).Distinct().ToList();
            var studentProfiles = await _unitOfWork.StudentProfiles.GetAllAsync();
            var myStudents = studentProfiles.Where(s => s.ClassRoomId.HasValue && myClassIds.Contains(s.ClassRoomId.Value)).ToList();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var allUsers = _userManager.Users.ToList();
            var studentExams = await _unitOfWork.StudentExams.GetAllAsync();
            var submissions = await _unitOfWork.AssignmentSubmissions.GetAllAsync();

            return myStudents.Select(s =>
            {
                var user = allUsers.FirstOrDefault(u => u.Id == s.UserId);
                var cr = classRooms.FirstOrDefault(c => c.Id == s.ClassRoomId.GetValueOrDefault());
                var sExams = studentExams.Where(se => se.StudentId == s.Id && se.IsSubmitted).ToList();
                var avgScore = sExams.Any() ? sExams.Average(e => e.Score) : 0;
                var riskLevel = avgScore > 0 && avgScore < 50 ? "High" : avgScore < 70 ? "Medium" : "Low";

                return new StudentSummaryDto
                {
                    StudentProfileId = s.Id,
                    StudentName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                    ClassName = cr?.Name ?? "N/A",
                    AverageScore = Math.Round(avgScore, 1),
                    AttendanceRate = 90.0,
                    RiskLevel = riskLevel
                };
            }).OrderBy(s => s.StudentName).ToList();
        }

        public async Task MarkAttendanceWithStatusAsync(Guid sessionId, Dictionary<Guid, string> studentStatuses)
        {
            var session = await _unitOfWork.AttendanceSessions.GetByIdAsync(sessionId);
            if (session == null) return;

            var existingRecords = await _unitOfWork.AttendanceRecords.GetAllAsync();
            var sessionRecords = existingRecords.Where(r => r.AttendanceSessionId == sessionId).ToList();

            foreach (var entry in studentStatuses)
            {
                var status = entry.Value switch
                {
                    "Absent" => Domain.Enums.AttendanceStatus.Absent,
                    "Late" => Domain.Enums.AttendanceStatus.Late,
                    "Excused" => Domain.Enums.AttendanceStatus.Excused,
                    _ => Domain.Enums.AttendanceStatus.Present
                };
                var isPresent = status == Domain.Enums.AttendanceStatus.Present || status == Domain.Enums.AttendanceStatus.Late;

                var existing = sessionRecords.FirstOrDefault(r => r.StudentId == entry.Key);
                if (existing != null)
                {
                    existing.Status = status;
                    existing.IsPresent = isPresent;
                    await _unitOfWork.AttendanceRecords.UpdateAsync(existing);
                }
                else
                {
                    await _unitOfWork.AttendanceRecords.AddAsync(new AttendanceRecord
                    {
                        Id = Guid.NewGuid(),
                        AttendanceSessionId = sessionId,
                        StudentId = entry.Key,
                        Status = status,
                        IsPresent = isPresent
                    });
                }
            }
            await _unitOfWork.SaveChangesAsync();
        }

        // ========== ASSIGNMENTS ==========

        public async Task<IEnumerable<AssignmentDto>> GetMyAssignmentsAsync(Guid teacherProfileId)
        {
            var assignments = await _unitOfWork.Assignments.GetAllAsync();
            var myAssignments = assignments.Where(a => a.TeacherId == teacherProfileId).ToList();
            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var submissions = await _unitOfWork.AssignmentSubmissions.GetAllAsync();

            return myAssignments.Select(a => new AssignmentDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                SubjectId = a.SubjectId,
                SubjectName = subjects.FirstOrDefault(s => s.Id == a.SubjectId)?.Name ?? "",
                ClassRoomId = a.ClassRoomId,
                ClassRoomName = classRooms.FirstOrDefault(c => c.Id == a.ClassRoomId)?.Name ?? "",
                TeacherId = a.TeacherId,
                TeacherName = "You",
                DueDate = a.DueDate,
                MaxScore = a.MaxScore,
                SubmissionCount = submissions.Count(s => s.AssignmentId == a.Id)
            }).OrderByDescending(a => a.DueDate).ToList();
        }

        public async Task<AssignmentDto> CreateAssignmentAsync(AssignmentDto dto, Guid teacherProfileId)
        {
            var entity = new Assignment
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                SubjectId = dto.SubjectId,
                ClassRoomId = dto.ClassRoomId,
                TeacherId = teacherProfileId,
                DueDate = dto.DueDate,
                MaxScore = dto.MaxScore > 0 ? dto.MaxScore : 100
            };
            await _unitOfWork.Assignments.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> DeleteAssignmentAsync(Guid id, Guid teacherProfileId)
        {
            var entity = await _unitOfWork.Assignments.GetByIdAsync(id);
            if (entity == null || entity.TeacherId != teacherProfileId) return false;
            await _unitOfWork.Assignments.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
