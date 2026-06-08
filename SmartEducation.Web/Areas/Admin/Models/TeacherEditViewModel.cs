using System.ComponentModel.DataAnnotations;

namespace SmartEducation.Web.Areas.Admin.Models
{
    public class TeacherEditViewModel
    {
        public Guid ProfileId { get; set; }
        public Guid UserId { get; set; }

        [Required] public string FirstName { get; set; } = default!;
        [Required] public string LastName { get; set; } = default!;
        [Required] public string Username { get; set; } = default!;
        [Required, EmailAddress] public string Email { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public string EmployeeNumber { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public string? Qualification { get; set; }
        public int? YearsOfExperience { get; set; }
    }
}
