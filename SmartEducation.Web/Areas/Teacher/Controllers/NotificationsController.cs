using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Constants;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Web.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = Roles.Teacher)]
    public class NotificationsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

            var allNotifications = await _unitOfWork.Notifications.GetAllAsync();
            var myNotifications = allNotifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.Id)
                .ToList();

            return View(myNotifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                await _unitOfWork.Notifications.UpdateAsync(notification);
                await _unitOfWork.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Index));

            var allNotifications = await _unitOfWork.Notifications.GetAllAsync();
            var myUnread = allNotifications.Where(n => n.UserId == user.Id && !n.IsRead).ToList();
            foreach (var n in myUnread)
            {
                n.IsRead = true;
                await _unitOfWork.Notifications.UpdateAsync(n);
            }
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "All notifications marked as read.";
            return RedirectToAction(nameof(Index));
        }
    }
}
