using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Constants;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Web.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = Roles.Teacher)]
    public class DashboardController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ITeacherService teacherService, UserManager<ApplicationUser> userManager)
        {
            _teacherService = teacherService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

            var dashboard = await _teacherService.GetDashboardAsync(user.Id);
            return View(dashboard);
        }
    }
}
