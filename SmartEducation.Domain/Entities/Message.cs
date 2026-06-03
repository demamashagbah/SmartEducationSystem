using SmartEducation.Domain.Common;

namespace SmartEducation.Domain.Entities
{
    public class Message : BaseEntity
    {
        public Guid SenderId { get; set; }
        public ApplicationUser Sender { get; set; } = default!;

        public Guid ReceiverId { get; set; }
        public ApplicationUser Receiver { get; set; } = default!;

        public string Subject { get; set; } = default!;

        public string Body { get; set; } = default!;

        public bool IsRead { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public Guid? ParentMessageId { get; set; }
        public Message? ParentMessage { get; set; }
    }
}
