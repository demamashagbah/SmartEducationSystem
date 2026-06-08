using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartEducation.Application.Constants;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Web.Areas.Admin.Models;

namespace SmartEducation.Web.Areas.Admin.Contollers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class UserManagementController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAcademicYearService _academicYearService;

        public UserManagementController(IUserService userService, IAcademicYearService academicYearService)
        {
            _userService = userService;
            _academicYearService = academicYearService;
        }

        public async Task<IActionResult> Index(string? role, string? search)
        {
            var users = string.IsNullOrEmpty(role)
                ? await _userService.GetAllAsync()
                : await _userService.GetByRoleAsync(role);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.ToLower();
                users = users.Where(u =>
                    u.FullName.ToLower().Contains(q) ||
                    u.Email.ToLower().Contains(q) ||
                    (u.PhoneNumber ?? "").Contains(q));
            }

            ViewBag.CurrentRole = role;
            ViewBag.Search = search ?? "";
            return View(users.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateUserFullViewModel
            {
                AcademicYearOptions = await GetAcademicYearOptions()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserFullViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AcademicYearOptions = await GetAcademicYearOptions();
                return View(model);
            }

            var (success, errors) = await _userService.CreateFullUserAsync(new CreateUserFullDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Username = model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                Role = model.Role,
                Password = model.Password,
                StudentNumber = model.StudentNumber,
                NationalNumber = model.NationalNumber,
                AcademicYearId = model.AcademicYearId,
                EnrollmentDate = model.EnrollmentDate,
                ParentName = model.ParentName,
                ParentPhone = model.ParentPhone,
                ParentEmail = model.ParentEmail,
                Address = model.Address,
                EmergencyContact = model.EmergencyContact,
                Specialization = model.Specialization,
                Qualification = model.Qualification,
                YearsOfExperience = model.YearsOfExperience,
                Occupation = model.Occupation,
                Position = model.Position,
                Department = model.Department
            });

            if (!success)
            {
                foreach (var e in errors) ModelState.AddModelError("", e);
                model.AcademicYearOptions = await GetAcademicYearOptions();
                return View(model);
            }

            TempData["Success"] = $"{model.Role} account created successfully.";

            return model.Role switch
            {
                Roles.Student => RedirectToAction("Index", "Student"),
                Roles.Teacher => RedirectToAction("Index", "Teacher"),
                Roles.Parent => RedirectToAction("Index", "Parent"),
                _ => RedirectToAction(nameof(Index))
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(Guid id, string? returnRole)
        {
            await _userService.ToggleActiveAsync(id);
            TempData["Success"] = "User status updated.";
            return RedirectToAction(nameof(Index), new { role = returnRole });
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(Guid id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            ViewBag.UserId = id;
            ViewBag.UserName = user.FullName;
            ViewBag.UserRole = user.Role;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(Guid id, string newPassword)
        {
            var (success, errors) = await _userService.ResetPasswordAsync(id, newPassword);
            TempData[success ? "Success" : "Error"] = success
                ? "Password reset successfully."
                : string.Join(", ", errors);
            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> GetAcademicYearOptions()
        {
            var years = await _academicYearService.GetAllAsync();
            return years.Select(y => new SelectListItem(y.Name, y.Id.ToString()));
        }
    }
}
