namespace SmartEducation.Application.DTOs
{
    public class LearningOutcomeDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = default!;
        public Guid TopicId { get; set; }
        public string TopicName { get; set; } = default!;
    }
}
