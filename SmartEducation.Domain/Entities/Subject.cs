using SmartEducation.Domain.Common;

namespace SmartEducation.Domain.Entities
{
    public class Subject : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public Guid? ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }

        public ICollection<Unit> Units { get; set; } = new List<Unit>();
    }
}
