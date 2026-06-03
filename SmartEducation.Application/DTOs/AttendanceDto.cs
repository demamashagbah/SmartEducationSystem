using SmartEducation.Domain.Enums;

namespace SmartEducation.Application.DTOs
{
    public class AttendanceSessionDto
    {
        public Guid Id { get; set; }
        public Guid ClassRoomId { get; set; }
        public string ClassRoomName { get; set; } = default!;
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = default!;
        public DateTime SessionDate { get; set; }
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
    }

    public class AttendanceRecordDto
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = default!;
        public AttendanceStatus Status { get; set; }
        public string? Notes { get; set; }
    }
}
