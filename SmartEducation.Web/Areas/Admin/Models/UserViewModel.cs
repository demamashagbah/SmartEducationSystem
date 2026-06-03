using System.ComponentModel.DataAnnotations;

namespace SmartEducation.Web.Areas.Admin.Models
{
    public class UserViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = default!;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = default!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = default!;

        [Required]
        public string Role { get; set; } = default!;
    }
}
