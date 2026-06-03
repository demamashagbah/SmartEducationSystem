using SmartEducation.Domain.Common;

namespace SmartEducation.Domain.Entities
{
    public class Assignment : BaseEntity
    {
        public string Title { get; set; } = default!;

        public string? Description { get; set; }

        public Guid SubjectId { get; set; }
        public Subject Subject { get; set; } = default!;

        public Guid ClassRoomId { get; set; }
        public ClassRoom ClassRoom { get; set; } = default!;

        public Guid TeacherId { get; set; }
        public TeacherProfile Teacher { get; set; } = default!;

        public DateTime DueDate { get; set; }

        public int MaxScore { get; set; } = 100;

        public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
    }
}
