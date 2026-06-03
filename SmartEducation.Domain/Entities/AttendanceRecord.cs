using SmartEducation.Domain.Common;
using SmartEducation.Domain.Enums;

namespace SmartEducation.Domain.Entities
{
    public class AttendanceRecord : BaseEntity
    {
        public Guid AttendanceSessionId { get; set; }
        public AttendanceSession AttendanceSession { get; set; } = default!;

        public Guid StudentId { get; set; }
        public StudentProfile Student { get; set; } = default!;

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        public bool IsPresent { get; set; }

        public string? Notes { get; set; }
    }
}
