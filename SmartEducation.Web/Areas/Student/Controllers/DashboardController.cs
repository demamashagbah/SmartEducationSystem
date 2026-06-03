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
    public class DashboardController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(IStudentService studentService, UserManager<ApplicationUser> userManager)
        {
            _studentService = studentService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

            var dashboard = await _studentService.GetDashboardAsync(user.Id);
            return View(dashboard);
        }
    }
}
