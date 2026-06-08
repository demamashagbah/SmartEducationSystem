using SmartEducation.Domain.Common;

namespace SmartEducation.Domain.Entities
{
    public class TeacherProfile : BaseEntity
    {
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string EmployeeNumber { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public string? Qualification { get; set; }
        public int? YearsOfExperience { get; set; }
    }
}
