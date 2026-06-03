using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartEducation.Application.Constants;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Web.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = Roles.Teacher)]
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessageController(IMessageService messageService, UserManager<ApplicationUser> userManager)
        {
            _messageService = messageService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

            var inbox = await _messageService.GetInboxAsync(user.Id);
            return View(inbox);
        }

        public async Task<IActionResult> Sent()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

            var sent = await _messageService.GetSentAsync(user.Id);
            return View(sent);
        }

        public async Task<IActionResult> Read(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

            var message = await _messageService.GetByIdAsync(id, user.Id);
            if (message == null) return NotFound();

            if (!message.IsRead && message.ReceiverId == user.Id)
                await _messageService.MarkAsReadAsync(id, user.Id);

            return View(message);
        }

        [HttpGet]
        public async Task<IActionResult> Compose(Guid? replyTo = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

            var dto = new SendMessageDto();
            if (replyTo.HasValue)
            {
                var original = await _messageService.GetByIdAsync(replyTo.Value, user.Id);
                if (original != null)
                {
                    dto.ReceiverId = original.SenderId;
                    dto.Subject = $"Re: {original.Subject}";
                    dto.ParentMessageId = original.Id;
                    ViewBag.ReplyToName = original.SenderName;
                }
            }

            await PopulateRecipients(user.Id);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Compose(SendMessageDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

            if (!ModelState.IsValid)
            {
                await PopulateRecipients(user.Id);
                return View(dto);
            }

            await _messageService.SendAsync(user.Id, dto);
            TempData["Success"] = "Message sent successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

            await _messageService.DeleteAsync(id, user.Id);
            TempData["Success"] = "Message deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateRecipients(Guid currentUserId)
        {
            var allUsers = _userManager.Users.Where(u => u.Id != currentUserId && u.IsActive).ToList();
            var recipients = new List<SelectListItem>();
            foreach (var u in allUsers.Take(50))
            {
                var roles = await _userManager.GetRolesAsync(u);
                var role = roles.FirstOrDefault() ?? "User";
                recipients.Add(new SelectListItem($"{u.FirstName} {u.LastName} ({role})", u.Id.ToString()));
            }
            ViewBag.Recipients = recipients;
        }
    }
}
