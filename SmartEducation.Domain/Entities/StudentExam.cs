using SmartEducation.Domain.Common;

namespace SmartEducation.Domain.Entities
{
    public class StudentExam : BaseEntity
    {
        public Guid StudentId { get; set; }
        public StudentProfile Student { get; set; } = default!;

        public Guid ExamId { get; set; }
        public Exam Exam { get; set; } = default!;

        public double Score { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? FinishedAt { get; set; }

        public bool IsSubmitted { get; set; } = false;
    }
}
