using Microsoft.AspNetCore.Identity;

namespace SmartEducation.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public bool IsActive { get; set; } = true;

        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Position { get; set; }
        public string? Department { get; set; }

        public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
    }
}
