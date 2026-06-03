using SmartEducation.Domain.Common;

namespace SmartEducation.Domain.Entities
{
    public class AIRecommendation : BaseEntity
    {
        public Guid StudentId { get; set; }
        public StudentProfile Student { get; set; } = default!;

        public string Recommendation { get; set; } = default!;

        public string Category { get; set; } = default!;

        public string? Priority { get; set; }

        public bool IsActedOn { get; set; } = false;
    }
}
