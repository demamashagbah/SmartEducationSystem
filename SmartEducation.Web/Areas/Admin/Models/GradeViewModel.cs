using System.ComponentModel.DataAnnotations;

namespace SmartEducation.Web.Areas.Admin.Models
{
    public class GradeViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Grade name is required")]
        [StringLength(100)]
        public string Name { get; set; } = default!;

        public int ClassRoomCount { get; set; }
    }
}
