using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Constants;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Web.Areas.Admin.Models;

namespace SmartEducation.Web.Areas.Admin.Contollers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class ParentController : Controller
    {
        private readonly IParentManagementService _parentService;

        public ParentController(IParentManagementService parentService)
        {
            _parentService = parentService;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var parents = await _parentService.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.ToLower();
                parents = parents.Where(p =>
                    p.FullName.ToLower().Contains(q) ||
                    p.Email.ToLower().Contains(q) ||
                    (p.PhoneNumber ?? "").ToLower().Contains(q));
            }

            ViewBag.Search = search ?? "";
            return View(parents.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var dto = await _parentService.GetByProfileIdAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var dto = await _parentService.GetByProfileIdAsync(id);
            if (dto == null) return NotFound();
            return View(MapToViewModel(dto));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ParentEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, errors) = await _parentService.UpdateParentAsync(new ParentDetailDto
            {
                ProfileId = model.ProfileId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Username = model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                Occupation = model.Occupation,
                EmergencyContact = model.EmergencyContact
            });

            if (!success)
            {
                foreach (var e in errors) ModelState.AddModelError("", e);
                return View(model);
            }

            TempData["Success"] = "Parent updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            await _parentService.DeactivateAsync(id);
            TempData["Success"] = "Parent status updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(Guid id)
        {
            var dto = await _parentService.GetByProfileIdAsync(id);
            if (dto == null) return NotFound();
            ViewBag.ParentName = dto.FullName;
            ViewBag.UserId = dto.UserId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(Guid userId, string newPassword)
        {
            var (success, errors) = await _parentService.ResetPasswordAsync(userId, newPassword);
            TempData[success ? "Success" : "Error"] = success
                ? "Password reset successfully."
                : string.Join(", ", errors);
            return RedirectToAction(nameof(Index));
        }

        private static ParentEditViewModel MapToViewModel(ParentDetailDto dto) => new()
        {
            ProfileId = dto.ProfileId,
            UserId = dto.UserId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Username = dto.Username,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            Occupation = dto.Occupation,
            EmergencyContact = dto.EmergencyContact
        };
    }
}
