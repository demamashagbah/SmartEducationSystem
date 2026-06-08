using SmartEducation.Domain.Common;

namespace SmartEducation.Domain.Entities
{
    public class ParentProfile : BaseEntity
    {
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string? Occupation { get; set; }
        public string? EmergencyContact { get; set; }

        public ICollection<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
    }
}
