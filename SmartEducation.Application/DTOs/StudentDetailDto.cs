namespace SmartEducation.Application.DTOs
{
    public class StudentDetailDto
    {
        public Guid ProfileId { get; set; }
        public Guid UserId { get; set; }

        // From ApplicationUser
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // From StudentProfile
        public string StudentNumber { get; set; } = string.Empty;
        public string NationalNumber { get; set; } = string.Empty;

        // Academic
        public Guid? ClassRoomId { get; set; }
        public string ClassRoomName { get; set; } = string.Empty;
        public Guid? AcademicYearId { get; set; }
        public string AcademicYearName { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }

        // Parent
        public string ParentName { get; set; } = string.Empty;
        public string ParentPhone { get; set; } = string.Empty;
        public string ParentEmail { get; set; } = string.Empty;

        // Optional
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }

        public bool IsActive { get; set; }
    }
}
