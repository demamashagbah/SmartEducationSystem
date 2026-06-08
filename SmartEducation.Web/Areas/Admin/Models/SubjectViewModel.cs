using System.ComponentModel.DataAnnotations;

namespace SmartEducation.Web.Areas.Admin.Models
{
    public class SubjectViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Subject name is required")]
        [StringLength(100)]
        public string Name { get; set; } = default!;

        [StringLength(500)]
        public string? Description { get; set; }

        public Guid ClassRoomId { get; set; }
        public string ClassRoomName { get; set; } = string.Empty;

        public int UnitCount { get; set; }
    }
}
