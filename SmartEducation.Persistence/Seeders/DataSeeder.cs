using Microsoft.EntityFrameworkCore;
using SmartEducation.Domain.Entities;
using SmartEducation.Persistence.Contexts;

namespace SmartEducation.Persistence.Seeders
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Grades.AnyAsync()) return;

            // Academic Years
            var academicYear = new AcademicYear
            {
                Id = Guid.NewGuid(),
                Name = "2025-2026",
                StartDate = new DateTime(2025, 9, 1),
                EndDate = new DateTime(2026, 6, 30)
            };
            await context.AcademicYears.AddAsync(academicYear);

            // Grades
            var grade10 = new Grade { Id = Guid.NewGuid(), Name = "Grade 10" };
            var grade11 = new Grade { Id = Guid.NewGuid(), Name = "Grade 11" };
            var grade12 = new Grade { Id = Guid.NewGuid(), Name = "Grade 12" };
            await context.Grades.AddRangeAsync(grade10, grade11, grade12);

            // ClassRooms
            var class10A = new ClassRoom { Id = Guid.NewGuid(), Name = "10-A", GradeId = grade10.Id };
            var class10B = new ClassRoom { Id = Guid.NewGuid(), Name = "10-B", GradeId = grade10.Id };
            var class11A = new ClassRoom { Id = Guid.NewGuid(), Name = "11-A", GradeId = grade11.Id };
            await context.ClassRooms.AddRangeAsync(class10A, class10B, class11A);

            // Subjects
            var mathSubject = new Subject { Id = Guid.NewGuid(), Name = "Mathematics", Description = "Core mathematics curriculum" };
            var scienceSubject = new Subject { Id = Guid.NewGuid(), Name = "Science", Description = "General science and natural sciences" };
            var englishSubject = new Subject { Id = Guid.NewGuid(), Name = "English Language", Description = "English language and literature" };
            var arabicSubject = new Subject { Id = Guid.NewGuid(), Name = "Arabic Language", Description = "Arabic language and grammar" };
            await context.Subjects.AddRangeAsync(mathSubject, scienceSubject, englishSubject, arabicSubject);

            // Units for Math
            var unit1 = new Domain.Entities.Unit { Id = Guid.NewGuid(), Name = "Unit 1 - Numbers and Operations", SubjectId = mathSubject.Id };
            var unit2 = new Domain.Entities.Unit { Id = Guid.NewGuid(), Name = "Unit 2 - Algebra", SubjectId = mathSubject.Id };
            var unit3 = new Domain.Entities.Unit { Id = Guid.NewGuid(), Name = "Unit 3 - Geometry", SubjectId = mathSubject.Id };
            await context.Units.AddRangeAsync(unit1, unit2, unit3);

            // Lessons for Unit 1
            var lesson1 = new Lesson { Id = Guid.NewGuid(), Name = "Lesson 1 - Real Numbers", UnitId = unit1.Id };
            var lesson2 = new Lesson { Id = Guid.NewGuid(), Name = "Lesson 2 - Integer Operations", UnitId = unit1.Id };
            var lesson3 = new Lesson { Id = Guid.NewGuid(), Name = "Lesson 3 - Fractions and Decimals", UnitId = unit1.Id };
            await context.Lessons.AddRangeAsync(lesson1, lesson2, lesson3);

            // Topics for Lesson 1
            var topic1 = new Topic { Id = Guid.NewGuid(), Name = "Types of Real Numbers", LessonId = lesson1.Id };
            var topic2 = new Topic { Id = Guid.NewGuid(), Name = "Number Line Representation", LessonId = lesson1.Id };
            await context.Topics.AddRangeAsync(topic1, topic2);

            // Learning Outcomes
            var lo1 = new LearningOutcome { Id = Guid.NewGuid(), Description = "Student can classify numbers as rational or irrational", TopicId = topic1.Id };
            var lo2 = new LearningOutcome { Id = Guid.NewGuid(), Description = "Student can represent real numbers on a number line", TopicId = topic2.Id };
            await context.LearningOutcomes.AddRangeAsync(lo1, lo2);

            // Semester
            var semester1 = new Semester
            {
                Id = Guid.NewGuid(),
                Name = "First Semester",
                AcademicYearId = academicYear.Id
            };
            var semester2 = new Semester
            {
                Id = Guid.NewGuid(),
                Name = "Second Semester",
                AcademicYearId = academicYear.Id
            };
            await context.Semesters.AddRangeAsync(semester1, semester2);

            await context.SaveChangesAsync();
        }
    }
}
