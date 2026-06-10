using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SmartEducation.Web.Areas.Admin.Models
{
    public class StudentViewModel
    {
        public Guid ProfileId { get; set; }
        public Guid UserId { get; set; }

        // Personal
        [Required] public string FirstName { get; set; } = default!;
        [Required] public string LastName { get; set; } = default!;
        [Required] public string Username { get; set; } = default!;
        [Required, EmailAddress] public string Email { get; set; } = default!;
        [Required] public string StudentNumber { get; set; } = default!;
        [Required] public string NationalNumber { get; set; } = default!;
        [Required] public string Gender { get; set; } = default!;
        [Required] public DateTime DateOfBirth { get; set; }

        // Password (only required on create)
        public string? Password { get; set; }

        // Academic
        [Required] public Guid ClassRoomId { get; set; }
        public string ClassRoomName { get; set; } = string.Empty;
        public Guid? AcademicYearId { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.Today;

        // Parent
        [Required] public string ParentName { get; set; } = default!;
        [Required] public string ParentPhone { get; set; } = default!;
        [Required, EmailAddress] public string ParentEmail { get; set; } = default!;

        // Optional
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }

        // Parent linking (optional: link to existing parent account)
        public Guid? LinkedParentProfileId { get; set; }
        public string? LinkedParentName { get; set; }

        // Dropdowns
        public IEnumerable<SelectListItem> AcademicYearOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ClassRoomOptions { get; set; } = new List<SelectListItem>();
    }
}
