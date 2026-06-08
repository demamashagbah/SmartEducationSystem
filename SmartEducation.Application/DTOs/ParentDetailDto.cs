namespace SmartEducation.Application.DTOs
{
    public class ParentDetailDto
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

        public string? Occupation { get; set; }
        public string? EmergencyContact { get; set; }

        public bool IsActive { get; set; }
        public List<StudentDetailDto> Children { get; set; } = new();
    }
}
