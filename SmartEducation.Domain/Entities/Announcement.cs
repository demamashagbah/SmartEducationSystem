using SmartEducation.Domain.Common;

namespace SmartEducation.Domain.Entities
{
    public class Announcement : BaseEntity
    {
        public string Title { get; set; } = default!;

        public string Content { get; set; } = default!;

        public Guid AuthorId { get; set; }

        public string? TargetRole { get; set; }

        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

        public bool IsPinned { get; set; } = false;
    }
}
