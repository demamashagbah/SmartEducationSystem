namespace SmartEducation.Application.DTOs
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = default!;
        public string SenderRole { get; set; } = default!;
        public Guid ReceiverId { get; set; }
        public string ReceiverName { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public string Body { get; set; } = default!;
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public Guid? ParentMessageId { get; set; }
    }

    public class SendMessageDto
    {
        public Guid ReceiverId { get; set; }
        public string Subject { get; set; } = default!;
        public string Body { get; set; } = default!;
        public Guid? ParentMessageId { get; set; }
    }
}
