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
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<LearningOutcome> LearningOutcomes { get; set; }

        // --- 5. (Exams & Question Banks) ---
        public DbSet<QuestionBank> QuestionBanks { get; set; }
        public DbSet<StudentExam> StudentExams { get; set; }


        // --- 6. (Assignments, Evaluations & Performance) ---
        public DbSet<TeacherAssignment> TeacherAssignments { get; set; }
        public DbSet<TeacherEvaluation> TeacherEvaluations { get; set; }
        public DbSet<StudentPerformance> StudentPerformances { get; set; }


        // --- 7. (Notifications) ---
        public DbSet<Notification> Notifications { get; set; }

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
        }
    }
}