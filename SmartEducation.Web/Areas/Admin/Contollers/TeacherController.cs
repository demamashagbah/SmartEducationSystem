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
    public class TeacherController : Controller
    {
        private readonly ITeacherManagementService _teacherService;

        public TeacherController(ITeacherManagementService teacherService)
        {
            _teacherService = teacherService;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var teachers = await _teacherService.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.ToLower();
                teachers = teachers.Where(t =>
                    t.FullName.ToLower().Contains(q) ||
                    t.Email.ToLower().Contains(q) ||
                    t.EmployeeNumber.ToLower().Contains(q) ||
                    (t.Specialization ?? "").ToLower().Contains(q));
            }

            ViewBag.Search = search ?? "";
            return View(teachers.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var dto = await _teacherService.GetByProfileIdAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var dto = await _teacherService.GetByProfileIdAsync(id);
            if (dto == null) return NotFound();
            return View(MapToViewModel(dto));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TeacherEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, errors) = await _teacherService.UpdateTeacherAsync(new TeacherDetailDto
            {
                ProfileId = model.ProfileId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Username = model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                Specialization = model.Specialization,
                Qualification = model.Qualification,
                YearsOfExperience = model.YearsOfExperience
            });

            if (!success)
            {
                foreach (var e in errors) ModelState.AddModelError("", e);
                return View(model);
            }

            TempData["Success"] = "Teacher updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            await _teacherService.DeactivateAsync(id);
            TempData["Success"] = "Teacher status updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(Guid id)
        {
            var dto = await _teacherService.GetByProfileIdAsync(id);
            if (dto == null) return NotFound();
            ViewBag.TeacherName = dto.FullName;
            ViewBag.UserId = dto.UserId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(Guid userId, string newPassword)
        {
            var (success, errors) = await _teacherService.ResetPasswordAsync(userId, newPassword);
            TempData[success ? "Success" : "Error"] = success
                ? "Password reset successfully."
                : string.Join(", ", errors);
            return RedirectToAction(nameof(Index));
        }

        private static TeacherEditViewModel MapToViewModel(TeacherDetailDto dto) => new()
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
            EmployeeNumber = dto.EmployeeNumber,
            Specialization = dto.Specialization,
            Qualification = dto.Qualification,
            YearsOfExperience = dto.YearsOfExperience
        };
    }
}
