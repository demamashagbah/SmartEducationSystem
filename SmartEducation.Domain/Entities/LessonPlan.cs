using SmartEducation.Domain.Common;

namespace SmartEducation.Domain.Entities
{
    public class LessonPlan : BaseEntity
    {
        public Guid TeacherAssignmentId { get; set; }
        public TeacherAssignment TeacherAssignment { get; set; } = default!;

        public Guid? LessonId { get; set; }
        public Lesson? Lesson { get; set; }

        public string Title { get; set; } = default!;

        public DateTime LessonDate { get; set; }

        public string Objectives { get; set; } = default!;

        public string? Introduction { get; set; }

        public string Activities { get; set; } = default!;

        public string? TeachingStrategies { get; set; }

        public string? AssessmentMethods { get; set; }

        public string? Homework { get; set; }

        public int DurationMinutes { get; set; } = 45;

        public bool IsAiGenerated { get; set; } = false;
    }
}
