namespace SmartEducation.Application.DTOs
{
    public class LessonDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public Guid UnitId { get; set; }
        public string UnitName { get; set; } = default!;
        public string SubjectName { get; set; } = default!;
        public int TopicCount { get; set; }
    }
}
