using SmartEducation.Domain.Common;

namespace SmartEducation.Domain.Entities
{
    public class AssignmentSubmission : BaseEntity
    {
        public Guid AssignmentId { get; set; }
        public Assignment Assignment { get; set; } = default!;

        public Guid StudentId { get; set; }
        public StudentProfile Student { get; set; } = default!;

        public string? FileUrl { get; set; }

        public string? Notes { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public double? Grade { get; set; }

        public string? Feedback { get; set; }
    }
}
