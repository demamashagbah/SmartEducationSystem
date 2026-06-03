using SmartEducation.Domain.Common;

namespace SmartEducation.Domain.Entities
{
    public class TeacherGuide : BaseEntity
    {
        public Guid SubjectId { get; set; }
        public Subject Subject { get; set; } = default!;

        public string FileName { get; set; } = default!;

        public string FilePath { get; set; } = default!;

        public string? Description { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public bool IsAnalyzed { get; set; } = false;

        public string? AnalysisNotes { get; set; }
    }
}
