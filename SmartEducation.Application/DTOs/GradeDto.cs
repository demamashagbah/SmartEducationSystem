namespace SmartEducation.Application.DTOs
{
    public class GradeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public int ClassRoomCount { get; set; }
    }
}
