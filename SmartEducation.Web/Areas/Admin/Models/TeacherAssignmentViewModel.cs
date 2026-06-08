using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SmartEducation.Web.Areas.Admin.Models
{
    public class TeacherAssignmentViewModel
    {
        [Required] public Guid TeacherId { get; set; }
        [Required] public Guid SubjectId { get; set; }
        [Required] public Guid ClassRoomId { get; set; }
        public Guid? AcademicYearId { get; set; }

        public IEnumerable<SelectListItem> TeacherOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> SubjectOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ClassRoomOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> AcademicYearOptions { get; set; } = new List<SelectListItem>();
    }
}
