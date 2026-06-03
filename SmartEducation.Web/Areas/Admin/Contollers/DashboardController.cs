using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Constants;
using SmartEducation.Application.Interfaces.Services;

namespace SmartEducation.Web.Areas.Admin.Contoller
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var stats = await _dashboardService.GetAdminStatisticsAsync();
            return View(stats);
        }

        public async Task<IActionResult> Analytics()
        {
            var analytics = await _dashboardService.GetAdminAnalyticsAsync();
            return View(analytics);
        }
    }
}
