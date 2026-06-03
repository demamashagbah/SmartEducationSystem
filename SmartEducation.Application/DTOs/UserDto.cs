namespace SmartEducation.Application.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}
