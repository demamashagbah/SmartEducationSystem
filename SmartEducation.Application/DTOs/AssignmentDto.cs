namespace SmartEducation.Application.DTOs
{
    public class AssignmentDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = default!;
        public Guid ClassRoomId { get; set; }
        public string ClassRoomName { get; set; } = default!;
        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; } = default!;
        public DateTime DueDate { get; set; }
        public int MaxScore { get; set; }
        public int SubmissionCount { get; set; }
    }
}
