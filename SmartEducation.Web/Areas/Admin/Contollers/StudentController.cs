using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Constants;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Web.Areas.Admin.Models;

namespace SmartEducation.Web.Areas.Admin.Contollers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class StudentController : Controller
    {
        private readonly IAdminStudentService _studentService;

        public StudentController(IAdminStudentService studentService)
        {
            _studentService = studentService;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var students = await _studentService.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.ToLower();
                students = students.Where(s =>
                    s.FullName.ToLower().Contains(q) ||
                    s.StudentNumber.ToLower().Contains(q) ||
                    s.NationalNumber.ToLower().Contains(q) ||
                    s.Email.ToLower().Contains(q));
            }

            ViewBag.Search = search ?? "";
            return View(students.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var dto = await _studentService.GetByProfileIdAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(Guid id)
        {
            var dto = await _studentService.GetByProfileIdAsync(id);
            if (dto == null) return NotFound();
            ViewBag.StudentName = dto.FullName;
            ViewBag.UserId = dto.UserId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(Guid userId, string newPassword)
        {
            var (success, errors) = await _studentService.ResetPasswordAsync(userId, newPassword);
            TempData[success ? "Success" : "Error"] = success
                ? "Password reset successfully."
                : string.Join(", ", errors);
            return RedirectToAction(nameof(Index));
        }
    }
}
