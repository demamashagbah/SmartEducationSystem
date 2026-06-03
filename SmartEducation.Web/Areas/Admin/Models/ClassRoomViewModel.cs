using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SmartEducation.Web.Areas.Admin.Models
{
    public class ClassRoomViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Class name is required")]
        [StringLength(100)]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "Please select a grade")]
        public Guid GradeId { get; set; }

        public string? GradeName { get; set; }

        public int StudentCount { get; set; }

        public IEnumerable<SelectListItem> GradeOptions { get; set; } = new List<SelectListItem>();
    }
}
