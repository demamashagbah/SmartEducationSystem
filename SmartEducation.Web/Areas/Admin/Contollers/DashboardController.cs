using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Constants;

namespace SmartEducation.Web.Areas.Admin.Contoller
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
