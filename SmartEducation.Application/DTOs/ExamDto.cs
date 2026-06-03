using SmartEducation.Domain.Enums;

namespace SmartEducation.Application.DTOs
{
    public class ExamDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = default!;
        public Guid? ClassRoomId { get; set; }
        public string? ClassRoomName { get; set; }
        public ExamType ExamType { get; set; }
        public DateTime ExamDate { get; set; }
        public int TotalMarks { get; set; }
        public int DurationMinutes { get; set; }
        public int QuestionCount { get; set; }
    }
}
