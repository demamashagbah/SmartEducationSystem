namespace SmartEducation.Application.DTOs
{
    public class TopicDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public Guid LessonId { get; set; }
        public string LessonName { get; set; } = default!;
        public int LearningOutcomeCount { get; set; }
    }
}
