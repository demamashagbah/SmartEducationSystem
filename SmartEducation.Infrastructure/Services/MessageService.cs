using Microsoft.AspNetCore.Identity;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Infrastructure.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessageService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IEnumerable<MessageDto>> GetInboxAsync(Guid userId)
        {
            var messages = await _unitOfWork.Messages.GetAllAsync();
            var inbox = messages.Where(m => m.ReceiverId == userId)
                                .OrderByDescending(m => m.SentAt)
                                .ToList();
            return await MapMessages(inbox);
        }

        public async Task<IEnumerable<MessageDto>> GetSentAsync(Guid userId)
        {
            var messages = await _unitOfWork.Messages.GetAllAsync();
            var sent = messages.Where(m => m.SenderId == userId)
                               .OrderByDescending(m => m.SentAt)
                               .ToList();
            return await MapMessages(sent);
        }

        public async Task<MessageDto?> GetByIdAsync(Guid messageId, Guid userId)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
            if (message == null) return null;
            if (message.SenderId != userId && message.ReceiverId != userId) return null;
            var mapped = await MapMessages(new[] { message });
            return mapped.FirstOrDefault();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            var messages = await _unitOfWork.Messages.GetAllAsync();
            return messages.Count(m => m.ReceiverId == userId && !m.IsRead);
        }

        public async Task<MessageDto> SendAsync(Guid senderId, SendMessageDto dto)
        {
            var message = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = dto.ReceiverId,
                Subject = dto.Subject,
                Body = dto.Body,
                SentAt = DateTime.UtcNow,
                IsRead = false,
                ParentMessageId = dto.ParentMessageId
            };
            await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            var sender = await _userManager.FindByIdAsync(senderId.ToString());
            var receiver = await _userManager.FindByIdAsync(dto.ReceiverId.ToString());

            return new MessageDto
            {
                Id = message.Id,
                SenderId = senderId,
                SenderName = sender != null ? $"{sender.FirstName} {sender.LastName}" : "Unknown",
                ReceiverId = dto.ReceiverId,
                ReceiverName = receiver != null ? $"{receiver.FirstName} {receiver.LastName}" : "Unknown",
                Subject = message.Subject,
                Body = message.Body,
                IsRead = false,
                SentAt = message.SentAt
            };
        }

        public async Task MarkAsReadAsync(Guid messageId, Guid userId)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
            if (message == null || message.ReceiverId != userId) return;
            message.IsRead = true;
            await _unitOfWork.Messages.UpdateAsync(message);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid messageId, Guid userId)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
            if (message == null) return;
            if (message.SenderId != userId && message.ReceiverId != userId) return;
            await _unitOfWork.Messages.DeleteAsync(message);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<IEnumerable<MessageDto>> MapMessages(IEnumerable<Message> messages)
        {
            var result = new List<MessageDto>();
            foreach (var m in messages)
            {
                var sender = await _userManager.FindByIdAsync(m.SenderId.ToString());
                var receiver = await _userManager.FindByIdAsync(m.ReceiverId.ToString());
                var senderRoles = sender != null ? await _userManager.GetRolesAsync(sender) : new List<string>();

                result.Add(new MessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = sender != null ? $"{sender.FirstName} {sender.LastName}" : "Unknown",
                    SenderRole = senderRoles.FirstOrDefault() ?? "",
                    ReceiverId = m.ReceiverId,
                    ReceiverName = receiver != null ? $"{receiver.FirstName} {receiver.LastName}" : "Unknown",
                    Subject = m.Subject,
                    Body = m.Body,
                    IsRead = m.IsRead,
                    SentAt = m.SentAt,
                    ParentMessageId = m.ParentMessageId
                });
            }
            return result;
        }
    }
}
