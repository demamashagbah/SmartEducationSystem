using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Constants;

namespace SmartEducation.Web.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = Roles.Teacher)]
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Settings";
            return View();
        }
    }
}
