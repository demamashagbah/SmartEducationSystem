using SmartEducation.Domain.Common;

namespace SmartEducation.Domain.Entities
{
    public class TeacherEvaluation : BaseEntity
    {
        public Guid TeacherId { get; set; }
        public TeacherProfile Teacher { get; set; } = default!;

        public Guid EvaluatorId { get; set; }

        public double Score { get; set; }

        public string? Notes { get; set; }

        public DateTime EvaluationDate { get; set; }
    }
}
