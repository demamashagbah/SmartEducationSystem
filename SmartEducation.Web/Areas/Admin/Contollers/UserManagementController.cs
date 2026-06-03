using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Constants;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;
using SmartEducation.Web.Areas.Admin.Models;

namespace SmartEducation.Web.Areas.Admin.Contollers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class UserManagementController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserManagementController(IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? role)
        {
            var users = string.IsNullOrEmpty(role)
                ? await _userService.GetAllAsync()
                : await _userService.GetByRoleAsync(role);

            var viewModels = users.Select(u => new UserViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.IsActive,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            });

            ViewBag.CurrentRole = role;
            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, errors) = await _userService.CreateUserAsync(
                model.FirstName, model.LastName, model.Email, model.Password, model.Role);

            if (!success)
            {
                foreach (var error in errors)
                    ModelState.AddModelError("", error);
                return View(model);
            }

            TempData["Success"] = "User created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            await _userService.ToggleActiveAsync(id);
            TempData["Success"] = "User status updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _userService.DeleteAsync(id);
            TempData["Success"] = "User deactivated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(Guid id, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
                ? "Password reset successfully."
                : string.Join(", ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }
    }
}
