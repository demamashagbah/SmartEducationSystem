using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Constants;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Web.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = Roles.Student)]
    public class AssignmentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AssignmentController(IStudentService studentService, UserManager<ApplicationUser> userManager)
        {
            _studentService = studentService;
            _userManager = userManager;
        }

        private async Task<StudentProfile?> GetStudentProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            return user != null ? await _studentService.GetProfileByUserIdAsync(user.Id) : null;
        }

        public async Task<IActionResult> Index()
        {
            var profile = await GetStudentProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var assignments = await _studentService.GetAssignmentsAsync(profile.Id);
            return View(assignments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Guid assignmentId, string? notes)
        {
            var profile = await GetStudentProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var submitted = await _studentService.SubmitAssignmentAsync(assignmentId, profile.Id, notes);
            TempData[submitted ? "Success" : "Error"] = submitted
                ? "Assignment submitted successfully!"
                : "Assignment already submitted.";

            return RedirectToAction(nameof(Index));
        }
    }
}
