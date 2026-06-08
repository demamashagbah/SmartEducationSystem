using SmartEducation.Domain.Common;

namespace SmartEducation.Domain.Entities
{
    public class TeacherAssignment : BaseEntity
    {
        public Guid TeacherId { get; set; }
        public TeacherProfile Teacher { get; set; }

        public Guid SubjectId { get; set; }
        public Subject Subject { get; set; }

        public Guid ClassRoomId { get; set; }
        public ClassRoom ClassRoom { get; set; }

        public Guid? AcademicYearId { get; set; }
        public AcademicYear? AcademicYear { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
