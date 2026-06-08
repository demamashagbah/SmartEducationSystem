using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartEducation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Persistence.Contexts
{
    public class ApplicationDbContext
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        Guid,
        IdentityUserClaim<Guid>,
        ApplicationUserRole,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // (Permissions & Security) ---
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }


        // --- 2. (User Profiles) ---
        
        public DbSet<TeacherProfile> TeacherProfiles { get; set; }
        public DbSet<StudentProfile> StudentProfiles { get; set; }
        public DbSet<ParentProfile> ParentProfiles { get; set; }


        // --- 3. (Junction Tables / Relations) ---
        public DbSet<ParentStudent> ParentStudents { get; set; }


        // --- 4.(Academic Structure & Curriculum) ---
        public DbSet<AcademicYear> AcademicYears { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<ClassRoom> ClassRooms { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<LearningOutcome> LearningOutcomes { get; set; }

        // --- 5. (Exams & Question Banks) ---
        public DbSet<Exam> Exams { get; set; }
        public DbSet<QuestionBank> QuestionBanks { get; set; }
        public DbSet<StudentExam> StudentExams { get; set; }

        // --- 6. (Assignments, Evaluations & Performance) ---
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        public DbSet<TeacherAssignment> TeacherAssignments { get; set; }
        public DbSet<TeacherEvaluation> TeacherEvaluations { get; set; }
        public DbSet<StudentPerformance> StudentPerformances { get; set; }

        // --- 7. (Attendance) ---
        public DbSet<AttendanceSession> AttendanceSessions { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

        // --- 8. (Lesson Plans) ---
        public DbSet<LessonPlan> LessonPlans { get; set; }

        // --- 9. (Notifications & Communication) ---
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Message> Messages { get; set; }

        // --- 10. (AI) ---
        public DbSet<AIInsight> AIInsights { get; set; }
        public DbSet<AIRecommendation> AIRecommendations { get; set; }

        // --- 11. (Curriculum Planning & Teacher Guides) ---
        public DbSet<TeacherGuide> TeacherGuides { get; set; }
        public DbSet<CurriculumPlan> CurriculumPlans { get; set; }
        public DbSet<CurriculumPlanItem> CurriculumPlanItems { get; set; }

        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUserRole>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId);

            builder.Entity<ApplicationUserRole>()
                .HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId);

            builder.Entity<ParentStudent>()
            .HasKey(ps => new { ps.ParentId, ps.StudentId });

            // 1. الطرف الأول (الطالب): عند حذفه، احذف سطر الربط تلقائياً (Cascade)
            builder.Entity<ParentStudent>()
                .HasOne(ps => ps.Student)
                .WithMany(s => s.ParentStudents)
                .HasForeignKey(ps => ps.StudentId)
                .OnDelete(DeleteBehavior.Cascade); // منطقي وتلقائي

            // 2. الطرف الثاني (ولي الأمر): نكسر السلسلة هنا لمنع الـ SQL Server من الاعتراض
            builder.Entity<ParentStudent>()
                .HasOne(ps => ps.Parent)
                .WithMany(p => p.ParentStudents)
                .HasForeignKey(ps => ps.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // Educational hierarchy — restrict cascades to prevent SQL Server cycle errors
            builder.Entity<Unit>()
                .HasOne(u => u.Subject)
                .WithMany(s => s.Units)
                .HasForeignKey(u => u.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Lesson>()
                .HasOne(l => l.Unit)
                .WithMany(u => u.Lessons)
                .HasForeignKey(l => l.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Topic>()
                .HasOne(t => t.Lesson)
                .WithMany(l => l.Topics)
                .HasForeignKey(t => t.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LearningOutcome>()
                .HasOne(lo => lo.Topic)
                .WithMany(t => t.LearningOutcomes)
                .HasForeignKey(lo => lo.TopicId)
                .OnDelete(DeleteBehavior.Restrict);

            // ClassRoom -> Grade
            builder.Entity<ClassRoom>()
                .HasOne(c => c.Grade)
                .WithMany(g => g.ClassRooms)
                .HasForeignKey(c => c.GradeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Semester -> AcademicYear
            builder.Entity<Semester>()
                .HasOne(s => s.AcademicYear)
                .WithMany()
                .HasForeignKey(s => s.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentProfile -> ClassRoom (nullable — student may not be assigned to a class yet)
            builder.Entity<StudentProfile>()
                .HasOne(sp => sp.ClassRoom)
                .WithMany()
                .HasForeignKey(sp => sp.ClassRoomId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // TeacherAssignment -> Subject, ClassRoom
            builder.Entity<TeacherAssignment>()
                .HasOne(ta => ta.Subject)
                .WithMany()
                .HasForeignKey(ta => ta.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TeacherAssignment>()
                .HasOne(ta => ta.ClassRoom)
                .WithMany()
                .HasForeignKey(ta => ta.ClassRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TeacherAssignment>()
                .HasOne(ta => ta.Teacher)
                .WithMany()
                .HasForeignKey(ta => ta.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // QuestionBank -> Subject and Exam
            builder.Entity<QuestionBank>()
                .HasOne(q => q.Subject)
                .WithMany()
                .HasForeignKey(q => q.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<QuestionBank>()
                .HasOne(q => q.Exam)
                .WithMany(e => e.Questions)
                .HasForeignKey(q => q.ExamId)
                .OnDelete(DeleteBehavior.SetNull);

            // Exam -> Subject
            builder.Entity<Exam>()
                .HasOne(e => e.Subject)
                .WithMany()
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Exam -> ClassRoom (optional)
            builder.Entity<Exam>()
                .HasOne(e => e.ClassRoom)
                .WithMany()
                .HasForeignKey(e => e.ClassRoomId)
                .OnDelete(DeleteBehavior.SetNull);

            // Exam -> StudentExam
            builder.Entity<StudentExam>()
                .HasOne(se => se.Exam)
                .WithMany(e => e.StudentExams)
                .HasForeignKey(se => se.ExamId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentExam -> StudentProfile
            builder.Entity<StudentExam>()
                .HasOne(se => se.Student)
                .WithMany()
                .HasForeignKey(se => se.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Assignment -> Subject, ClassRoom, Teacher
            builder.Entity<Assignment>()
                .HasOne(a => a.Subject)
                .WithMany()
                .HasForeignKey(a => a.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Assignment>()
                .HasOne(a => a.ClassRoom)
                .WithMany()
                .HasForeignKey(a => a.ClassRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Assignment>()
                .HasOne(a => a.Teacher)
                .WithMany()
                .HasForeignKey(a => a.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Assignment -> AssignmentSubmission
            builder.Entity<AssignmentSubmission>()
                .HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AssignmentSubmission>()
                .HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // AttendanceSession -> ClassRoom, Subject, Teacher
            builder.Entity<AttendanceSession>()
                .HasOne(s => s.ClassRoom)
                .WithMany()
                .HasForeignKey(s => s.ClassRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AttendanceSession>()
                .HasOne(s => s.Subject)
                .WithMany()
                .HasForeignKey(s => s.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AttendanceSession>()
                .HasOne(s => s.Teacher)
                .WithMany()
                .HasForeignKey(s => s.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // AttendanceRecord -> AttendanceSession, Student
            builder.Entity<AttendanceRecord>()
                .HasOne(r => r.AttendanceSession)
                .WithMany(s => s.Records)
                .HasForeignKey(r => r.AttendanceSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AttendanceRecord>()
                .HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // TeacherEvaluation -> TeacherProfile
            builder.Entity<TeacherEvaluation>()
                .HasOne(te => te.Teacher)
                .WithMany()
                .HasForeignKey(te => te.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // AIRecommendation -> StudentProfile
            builder.Entity<AIRecommendation>()
                .HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // LessonPlan -> TeacherAssignment and Lesson
            builder.Entity<LessonPlan>()
                .HasOne(lp => lp.TeacherAssignment)
                .WithMany()
                .HasForeignKey(lp => lp.TeacherAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LessonPlan>()
                .HasOne(lp => lp.Lesson)
                .WithMany()
                .HasForeignKey(lp => lp.LessonId)
                .OnDelete(DeleteBehavior.SetNull);

            // Message -> Sender, Receiver (no cascade to avoid multiple paths)
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.ParentMessage)
                .WithMany()
                .HasForeignKey(m => m.ParentMessageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Subject -> ClassRoom (nullable: subjects can exist without a class during migration)
            builder.Entity<Subject>()
                .HasOne(s => s.ClassRoom)
                .WithMany(c => c.Subjects)
                .HasForeignKey(s => s.ClassRoomId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // StudentProfile -> AcademicYear (optional)
            builder.Entity<StudentProfile>()
                .HasOne(sp => sp.AcademicYear)
                .WithMany()
                .HasForeignKey(sp => sp.AcademicYearId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // TeacherAssignment -> AcademicYear (optional)
            builder.Entity<TeacherAssignment>()
                .HasOne(ta => ta.AcademicYear)
                .WithMany()
                .HasForeignKey(ta => ta.AcademicYearId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // TeacherGuide -> Subject
            builder.Entity<TeacherGuide>()
                .HasOne(tg => tg.Subject)
                .WithMany()
                .HasForeignKey(tg => tg.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // CurriculumPlan -> TeacherAssignment
            builder.Entity<CurriculumPlan>()
                .HasOne(cp => cp.TeacherAssignment)
                .WithMany()
                .HasForeignKey(cp => cp.TeacherAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // CurriculumPlanItem -> CurriculumPlan
            builder.Entity<CurriculumPlanItem>()
                .HasOne(cpi => cpi.CurriculumPlan)
                .WithMany(cp => cp.Items)
                .HasForeignKey(cpi => cpi.CurriculumPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // CurriculumPlanItem -> Lesson (optional)
            builder.Entity<CurriculumPlanItem>()
                .HasOne(cpi => cpi.Lesson)
                .WithMany()
                .HasForeignKey(cpi => cpi.LessonId)
                .OnDelete(DeleteBehavior.SetNull);

            // CurriculumPlanItem -> Topic (optional)
            builder.Entity<CurriculumPlanItem>()
                .HasOne(cpi => cpi.Topic)
                .WithMany()
                .HasForeignKey(cpi => cpi.TopicId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}