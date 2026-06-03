using SmartEducation.Domain.Entities;

namespace SmartEducation.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Subject> Subjects { get; }
        IGenericRepository<Unit> Units { get; }
        IGenericRepository<Lesson> Lessons { get; }
        IGenericRepository<Topic> Topics { get; }
        IGenericRepository<LearningOutcome> LearningOutcomes { get; }
        IGenericRepository<Grade> Grades { get; }
        IGenericRepository<ClassRoom> ClassRooms { get; }
        IGenericRepository<AcademicYear> AcademicYears { get; }
        IGenericRepository<Semester> Semesters { get; }
        IGenericRepository<Exam> Exams { get; }
        IGenericRepository<QuestionBank> QuestionBanks { get; }
        IGenericRepository<StudentExam> StudentExams { get; }
        IGenericRepository<Assignment> Assignments { get; }
        IGenericRepository<AssignmentSubmission> AssignmentSubmissions { get; }
        IGenericRepository<TeacherAssignment> TeacherAssignments { get; }
        IGenericRepository<TeacherProfile> TeacherProfiles { get; }
        IGenericRepository<StudentProfile> StudentProfiles { get; }
        IGenericRepository<ParentProfile> ParentProfiles { get; }
        IGenericRepository<AttendanceSession> AttendanceSessions { get; }
        IGenericRepository<AttendanceRecord> AttendanceRecords { get; }
        IGenericRepository<LessonPlan> LessonPlans { get; }
        IGenericRepository<Notification> Notifications { get; }
        IGenericRepository<Announcement> Announcements { get; }
        IGenericRepository<StudentPerformance> StudentPerformances { get; }
        IGenericRepository<Message> Messages { get; }
        IGenericRepository<TeacherGuide> TeacherGuides { get; }
        IGenericRepository<CurriculumPlan> CurriculumPlans { get; }
        IGenericRepository<CurriculumPlanItem> CurriculumPlanItems { get; }

        Task<int> SaveChangesAsync();
    }
}
