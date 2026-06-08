namespace SmartEducation.Application.DTOs
{
    public class TeacherDetailDto
    {
        public Guid ProfileId { get; set; }
        public Guid UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public string EmployeeNumber { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public string? Qualification { get; set; }
        public int? YearsOfExperience { get; set; }

        public bool IsActive { get; set; }

        // Assigned classes / subjects (summary)
        public int AssignedClassCount { get; set; }
        public int AssignedSubjectCount { get; set; }
        public List<string> AssignedClassNames { get; set; } = new();
        public List<string> AssignedSubjectNames { get; set; } = new();
    }
}
