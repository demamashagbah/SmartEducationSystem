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
                var sender   = await _userManager.FindByIdAsync(m.SenderId.ToString());
                var receiver = await _userManager.FindByIdAsync(m.ReceiverId.ToString());
                var senderRoles = sender != null ? await _userManager.GetRolesAsync(sender) : new List<string>();

                result.Add(new MessageDto
                {
                    Id           = m.Id,
                    SenderId     = m.SenderId,
                    SenderName   = sender   != null ? $"{sender.FirstName} {sender.LastName}"   : "Unknown",
                    SenderRole   = senderRoles.FirstOrDefault() ?? "",
                    ReceiverId   = m.ReceiverId,
                    ReceiverName = receiver != null ? $"{receiver.FirstName} {receiver.LastName}" : "Unknown",
                    Subject      = m.Subject,
                    Body         = m.Body,
                    IsRead       = m.IsRead,
                    SentAt       = m.SentAt,
                    ParentMessageId = m.ParentMessageId
                });
            }
            return result;
        }

        // ══ Conversation (WhatsApp-style) methods ══════════════════════════

        public async Task<IEnumerable<ConversationDto>> GetConversationsAsync(Guid userId)
        {
            var all = await _unitOfWork.Messages.GetAllAsync();
            var mine = all.Where(m => m.SenderId == userId || m.ReceiverId == userId).ToList();

            // Group by the other participant
            var grouped = mine
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .ToList();

            var avatarColors = new[] { "#696cff", "#71dd37", "#fd7e14", "#e74c3c", "#20c997", "#9b59b6" };
            var result = new List<ConversationDto>();
            int colorIdx = 0;

            foreach (var group in grouped)
            {
                var otherId   = group.Key;
                var otherUser = await _userManager.FindByIdAsync(otherId.ToString());
                if (otherUser == null) continue;

                var roles     = await _userManager.GetRolesAsync(otherUser);
                var otherRole = roles.FirstOrDefault() ?? "";
                var otherName = $"{otherUser.FirstName} {otherUser.LastName}".Trim();
                var initials  = (otherUser.FirstName.Length > 0 ? otherUser.FirstName[0].ToString() : "?") +
                                (otherUser.LastName.Length  > 0 ? otherUser.LastName[0].ToString()  : "");

                var sorted    = group.OrderByDescending(m => m.SentAt).ToList();
                var latest    = sorted.First();
                var unread    = sorted.Count(m => m.ReceiverId == userId && !m.IsRead);

                result.Add(new ConversationDto
                {
                    OtherUserId         = otherId,
                    OtherUserName       = otherName,
                    OtherUserRole       = otherRole,
                    OtherUserInitials   = initials,
                    LastMessage         = latest.Body.Length > 60 ? latest.Body.Substring(0, 60) + "…" : latest.Body,
                    LastMessageAt       = latest.SentAt,
                    LastMessageIsFromMe = latest.SenderId == userId,
                    UnreadCount         = unread,
                    AvatarColor         = avatarColors[colorIdx++ % avatarColors.Length]
                });
            }

            return result.OrderByDescending(c => c.LastMessageAt).ToList();
        }

        public async Task<IEnumerable<ConversationMessageDto>> GetConversationAsync(Guid userId, Guid otherId)
        {
            var all  = await _unitOfWork.Messages.GetAllAsync();
            var conv = all.Where(m =>
                (m.SenderId == userId && m.ReceiverId == otherId) ||
                (m.SenderId == otherId && m.ReceiverId == userId))
                .OrderBy(m => m.SentAt)
                .ToList();

            var meUser    = await _userManager.FindByIdAsync(userId.ToString());
            var otherUser = await _userManager.FindByIdAsync(otherId.ToString());

            return conv.Select(m =>
            {
                var isFromMe  = m.SenderId == userId;
                var senderUser = isFromMe ? meUser : otherUser;
                var senderName = senderUser != null ? $"{senderUser.FirstName} {senderUser.LastName}".Trim() : "Unknown";
                var initials   = senderUser != null
                    ? (senderUser.FirstName.Length > 0 ? senderUser.FirstName[0].ToString() : "?") +
                      (senderUser.LastName.Length  > 0 ? senderUser.LastName[0].ToString()  : "")
                    : "?";

                var localTime = m.SentAt.ToLocalTime();
                var formatted = localTime.Date == DateTime.Today
                    ? localTime.ToString("HH:mm")
                    : localTime.Date == DateTime.Today.AddDays(-1)
                        ? "Yesterday " + localTime.ToString("HH:mm")
                        : localTime.ToString("MMM dd, HH:mm");

                return new ConversationMessageDto
                {
                    Id             = m.Id,
                    SenderId       = m.SenderId,
                    SenderName     = senderName,
                    SenderInitials = initials,
                    Body           = m.Body,
                    SentAt         = m.SentAt,
                    IsRead         = m.IsRead,
                    IsFromMe       = isFromMe,
                    FormattedTime  = formatted
                };
            }).ToList();
        }

        public async Task<ConversationMessageDto> SendChatAsync(Guid senderId, Guid receiverId, string body)
        {
            var message = new Message
            {
                Id         = Guid.NewGuid(),
                SenderId   = senderId,
                ReceiverId = receiverId,
                Subject    = "Chat",
                Body       = body,
                SentAt     = DateTime.UtcNow,
                IsRead     = false
            };
            await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            var sender = await _userManager.FindByIdAsync(senderId.ToString());
            var name   = sender != null ? $"{sender.FirstName} {sender.LastName}".Trim() : "Unknown";
            var initials = sender != null
                ? (sender.FirstName.Length > 0 ? sender.FirstName[0].ToString() : "?") +
                  (sender.LastName.Length  > 0 ? sender.LastName[0].ToString()  : "")
                : "?";

            var localTime = message.SentAt.ToLocalTime();
            return new ConversationMessageDto
            {
                Id             = message.Id,
                SenderId       = senderId,
                SenderName     = name,
                SenderInitials = initials,
                Body           = body,
                SentAt         = message.SentAt,
                IsRead         = false,
                IsFromMe       = true,
                FormattedTime  = localTime.ToString("HH:mm")
            };
        }

        public async Task MarkConversationReadAsync(Guid userId, Guid otherId)
        {
            var all  = await _unitOfWork.Messages.GetAllAsync();
            var unread = all.Where(m => m.SenderId == otherId && m.ReceiverId == userId && !m.IsRead).ToList();
            foreach (var m in unread)
            {
                m.IsRead = true;
                await _unitOfWork.Messages.UpdateAsync(m);
            }
            if (unread.Any())
                await _unitOfWork.SaveChangesAsync();
        }
    }
}
