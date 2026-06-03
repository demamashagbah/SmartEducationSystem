namespace SmartEducation.Application.DTOs
{
    public class LessonPlanDto
    {
        public Guid Id { get; set; }
        public Guid TeacherAssignmentId { get; set; }
        public Guid? LessonId { get; set; }
        public string? LessonName { get; set; }
        public string Title { get; set; } = default!;
        public DateTime LessonDate { get; set; }
        public string Objectives { get; set; } = default!;
        public string? Introduction { get; set; }
        public string Activities { get; set; } = default!;
        public string? TeachingStrategies { get; set; }
        public string? AssessmentMethods { get; set; }
        public string? Homework { get; set; }
        public int DurationMinutes { get; set; } = 45;
        public bool IsAiGenerated { get; set; }
        public string SubjectName { get; set; } = default!;
        public string ClassRoomName { get; set; } = default!;
        public Guid SubjectId { get; set; }
        public Guid ClassRoomId { get; set; }
    }
}
