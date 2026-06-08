using SmartEducation.Domain.Common;

namespace SmartEducation.Domain.Entities
{
    public class ClassRoom : BaseEntity
    {
        public string Name { get; set; }

        public Guid GradeId { get; set; }
        public Grade Grade { get; set; }

        public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    }
}
