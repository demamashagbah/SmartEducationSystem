using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartEducation.Application.Constants;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Web.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = Roles.Student)]
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessageController(IMessageService messageService, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _messageService = messageService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });
            var inbox = await _messageService.GetInboxAsync(user.Id);
            return View(inbox);
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
            TempData["Success"] = "Message sent.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateRecipients(Guid currentUserId)
        {
            var studentProfile = await _unitOfWork.StudentProfiles.GetAllAsync();
            var myProfile = studentProfile.FirstOrDefault(s => s.UserId == currentUserId);

            if (myProfile != null)
            {
                var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
                var myTeacherProfileIds = teacherAssignments
                    .Where(ta => ta.ClassRoomId == myProfile.ClassRoomId)
                    .Select(ta => ta.TeacherId).Distinct().ToList();
                var teacherProfiles = await _unitOfWork.TeacherProfiles.GetAllAsync();
                var myTeachers = teacherProfiles.Where(tp => myTeacherProfileIds.Contains(tp.Id)).ToList();
                var allUsers = _userManager.Users.ToList();

                var recipients = myTeachers.Select(tp =>
                {
                    var u = allUsers.FirstOrDefault(x => x.Id == tp.UserId);
                    return u != null
                        ? new SelectListItem($"{u.FirstName} {u.LastName} (Teacher)", u.Id.ToString())
                        : null;
                }).Where(x => x != null).ToList();

                ViewBag.Recipients = recipients;
            }
            else
            {
                ViewBag.Recipients = new List<SelectListItem>();
            }
        }
    }
}
