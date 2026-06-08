using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SmartEducation.Web.Areas.Admin.Models
{
    public class CreateUserFullViewModel
    {
        // General Information
        [Required] public string FirstName { get; set; } = default!;
        [Required] public string LastName { get; set; } = default!;
        [Required] public string Username { get; set; } = default!;
        [Required, EmailAddress] public string Email { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // Account
        [Required] public string Role { get; set; } = default!;
        [Required, MinLength(6)] public string Password { get; set; } = default!;
        [Required, Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = default!;

        // Student
        public string? StudentNumber { get; set; }
        public string? NationalNumber { get; set; }
        public Guid? AcademicYearId { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
        public string? ParentEmail { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }

        // Teacher
        public string? Specialization { get; set; }
        public string? Qualification { get; set; }
        public int? YearsOfExperience { get; set; }

        // Parent
        public string? Occupation { get; set; }

        // Admin
        public string? Position { get; set; }
        public string? Department { get; set; }

        // Dropdowns
        public IEnumerable<SelectListItem> AcademicYearOptions { get; set; } = new List<SelectListItem>();
    }
}
