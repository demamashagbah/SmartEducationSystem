using SmartEducation.Domain.Common;

namespace SmartEducation.Domain.Entities
{
    public class CurriculumPlanItem : BaseEntity
    {
        public Guid CurriculumPlanId { get; set; }
        public CurriculumPlan CurriculumPlan { get; set; } = default!;

        public Guid? LessonId { get; set; }
        public Lesson? Lesson { get; set; }

        public Guid? TopicId { get; set; }
        public Topic? Topic { get; set; }

        public int WeekNumber { get; set; }

        public DateTime PlannedDate { get; set; }

        public string? LearningOutcomes { get; set; }

        public string? TeachingStrategy { get; set; }

        public string? Activities { get; set; }

        public string? Homework { get; set; }

        public DateTime? QuizDate { get; set; }

        public DateTime? ExamDate { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedDate { get; set; }

        public string? Notes { get; set; }
    }
}
