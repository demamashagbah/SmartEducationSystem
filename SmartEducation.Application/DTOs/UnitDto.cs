namespace SmartEducation.Application.DTOs
{
    public class UnitDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = default!;
        public int LessonCount { get; set; }
    }
}
