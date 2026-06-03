using SmartEducation.Domain.Common;

namespace SmartEducation.Domain.Entities
{
    public class AttendanceSession : BaseEntity
    {
        public Guid ClassRoomId { get; set; }
        public ClassRoom ClassRoom { get; set; } = default!;

        public Guid SubjectId { get; set; }
        public Subject Subject { get; set; } = default!;

        public Guid TeacherId { get; set; }
        public TeacherProfile Teacher { get; set; } = default!;

        public DateTime SessionDate { get; set; }

        public ICollection<AttendanceRecord> Records { get; set; } = new List<AttendanceRecord>();
    }
}
