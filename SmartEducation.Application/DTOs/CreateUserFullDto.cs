namespace SmartEducation.Application.DTOs
{
    public class CreateUserFullDto
    {
        // General Information
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // Account
        public string Role { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Student-specific
        public string? StudentNumber { get; set; }
        public string? NationalNumber { get; set; }
        public Guid? AcademicYearId { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
        public string? ParentEmail { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }

        // Teacher-specific
        public string? Specialization { get; set; }
        public string? Qualification { get; set; }
        public int? YearsOfExperience { get; set; }

        // Parent-specific
        public string? Occupation { get; set; }

        // Admin-specific
        public string? Position { get; set; }
        public string? Department { get; set; }
    }
}
