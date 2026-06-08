namespace SmartEducation.Application.DTOs
{
    public class SubjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? ClassRoomId { get; set; }
        public string ClassRoomName { get; set; } = string.Empty;
    }
}
