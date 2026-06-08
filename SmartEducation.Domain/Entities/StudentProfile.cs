using SmartEducation.Domain.Common;

namespace SmartEducation.Domain.Entities
{
    public class StudentProfile : BaseEntity
    {
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        // Nullable: student may not yet be assigned to a class
        public Guid? ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }

        public string StudentNumber { get; set; } = string.Empty;
        public string NationalNumber { get; set; } = string.Empty;

        public Guid? AcademicYearId { get; set; }
        public AcademicYear? AcademicYear { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        public string ParentName { get; set; } = string.Empty;
        public string ParentPhone { get; set; } = string.Empty;
        public string ParentEmail { get; set; } = string.Empty;

        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }

        public ICollection<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
    }
}
