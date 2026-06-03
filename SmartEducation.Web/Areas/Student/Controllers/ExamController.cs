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
    public class ExamController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExamController(IStudentService studentService, UserManager<ApplicationUser> userManager)
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

            var exams = await _studentService.GetUpcomingExamsAsync(profile.Id);
            return View(exams);
        }
    }
}
