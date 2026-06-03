using SmartEducation.Application.DTOs;

namespace SmartEducation.Application.Interfaces.Services
{
    public interface IMessageService
    {
        Task<IEnumerable<MessageDto>> GetInboxAsync(Guid userId);
        Task<IEnumerable<MessageDto>> GetSentAsync(Guid userId);
        Task<MessageDto?> GetByIdAsync(Guid messageId, Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<MessageDto> SendAsync(Guid senderId, SendMessageDto dto);
        Task MarkAsReadAsync(Guid messageId, Guid userId);
        Task DeleteAsync(Guid messageId, Guid userId);
    }
}
