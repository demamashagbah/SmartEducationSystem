using SmartEducation.Application.Interfaces;
using SmartEducation.Domain.Entities;
using SmartEducation.Persistence.Contexts;
using SmartEducation.Persistence.Repositories;

namespace SmartEducation.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IGenericRepository<Subject> Subjects { get; }
        public IGenericRepository<Unit> Units { get; }
        public IGenericRepository<Lesson> Lessons { get; }
        public IGenericRepository<Topic> Topics { get; }
        public IGenericRepository<LearningOutcome> LearningOutcomes { get; }
        public IGenericRepository<Grade> Grades { get; }
        public IGenericRepository<ClassRoom> ClassRooms { get; }
        public IGenericRepository<AcademicYear> AcademicYears { get; }
        public IGenericRepository<Semester> Semesters { get; }
        public IGenericRepository<Exam> Exams { get; }
        public IGenericRepository<QuestionBank> QuestionBanks { get; }
        public IGenericRepository<StudentExam> StudentExams { get; }
        public IGenericRepository<Assignment> Assignments { get; }
        public IGenericRepository<AssignmentSubmission> AssignmentSubmissions { get; }
        public IGenericRepository<TeacherAssignment> TeacherAssignments { get; }
        public IGenericRepository<TeacherProfile> TeacherProfiles { get; }
        public IGenericRepository<StudentProfile> StudentProfiles { get; }
        public IGenericRepository<ParentProfile> ParentProfiles { get; }
        public IGenericRepository<AttendanceSession> AttendanceSessions { get; }
        public IGenericRepository<AttendanceRecord> AttendanceRecords { get; }
        public IGenericRepository<LessonPlan> LessonPlans { get; }
        public IGenericRepository<Notification> Notifications { get; }
        public IGenericRepository<Announcement> Announcements { get; }
        public IGenericRepository<StudentPerformance> StudentPerformances { get; }
        public IGenericRepository<Message> Messages { get; }
        public IGenericRepository<TeacherGuide> TeacherGuides { get; }
        public IGenericRepository<CurriculumPlan> CurriculumPlans { get; }
        public IGenericRepository<CurriculumPlanItem> CurriculumPlanItems { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Subjects = new GenericRepository<Subject>(context);
            Units = new GenericRepository<Unit>(context);
            Lessons = new GenericRepository<Lesson>(context);
            Topics = new GenericRepository<Topic>(context);
            LearningOutcomes = new GenericRepository<LearningOutcome>(context);
            Grades = new GenericRepository<Grade>(context);
            ClassRooms = new GenericRepository<ClassRoom>(context);
            AcademicYears = new GenericRepository<AcademicYear>(context);
            Semesters = new GenericRepository<Semester>(context);
            Exams = new GenericRepository<Exam>(context);
            QuestionBanks = new GenericRepository<QuestionBank>(context);
            StudentExams = new GenericRepository<StudentExam>(context);
            Assignments = new GenericRepository<Assignment>(context);
            AssignmentSubmissions = new GenericRepository<AssignmentSubmission>(context);
            TeacherAssignments = new GenericRepository<TeacherAssignment>(context);
            TeacherProfiles = new GenericRepository<TeacherProfile>(context);
            StudentProfiles = new GenericRepository<StudentProfile>(context);
            ParentProfiles = new GenericRepository<ParentProfile>(context);
            AttendanceSessions = new GenericRepository<AttendanceSession>(context);
            AttendanceRecords = new GenericRepository<AttendanceRecord>(context);
            LessonPlans = new GenericRepository<LessonPlan>(context);
            Notifications = new GenericRepository<Notification>(context);
            Announcements = new GenericRepository<Announcement>(context);
            StudentPerformances = new GenericRepository<StudentPerformance>(context);
            Messages = new GenericRepository<Message>(context);
            TeacherGuides = new GenericRepository<TeacherGuide>(context);
            CurriculumPlans = new GenericRepository<CurriculumPlan>(context);
            CurriculumPlanItems = new GenericRepository<CurriculumPlanItem>(context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
