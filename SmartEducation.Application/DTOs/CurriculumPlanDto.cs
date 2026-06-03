namespace SmartEducation.Application.DTOs
{
    public class CurriculumPlanDto
    {
        public Guid Id { get; set; }
        public Guid TeacherAssignmentId { get; set; }
        public string SubjectName { get; set; } = default!;
        public string ClassRoomName { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string PlanType { get; set; } = "Weekly";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsAiGenerated { get; set; } = true;
        public List<CurriculumPlanItemDto> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int CompletedItems { get; set; }
        public double ProgressPercent => TotalItems > 0 ? Math.Round((CompletedItems * 100.0) / TotalItems, 1) : 0;
    }

    public class CurriculumPlanItemDto
    {
        public Guid Id { get; set; }
        public Guid CurriculumPlanId { get; set; }
        public Guid? LessonId { get; set; }
        public string? LessonName { get; set; }
        public Guid? TopicId { get; set; }
        public string? TopicName { get; set; }
        public int WeekNumber { get; set; }
        public DateTime PlannedDate { get; set; }
        public string? LearningOutcomes { get; set; }
        public string? TeachingStrategy { get; set; }
        public string? Activities { get; set; }
        public string? Homework { get; set; }
        public DateTime? QuizDate { get; set; }
        public DateTime? ExamDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Notes { get; set; }
    }

    public class GenerateCurriculumPlanRequest
    {
        public Guid TeacherAssignmentId { get; set; }
        public string PlanType { get; set; } = "Weekly";
        public DateTime StartDate { get; set; }
        public int LessonsPerWeek { get; set; } = 2;
        public string? Notes { get; set; }
    }
}
