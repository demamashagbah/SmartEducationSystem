using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Constants;
using SmartEducation.Application.Interfaces;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Web.Areas.Parent.Controllers
{
    [Area("Parent")]
    [Authorize(Roles = Roles.Parent)]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

            ViewBag.ParentName = $"{user.FirstName} {user.LastName}";

            var parents = await _unitOfWork.ParentProfiles.GetAllAsync();
            var parentProfile = parents.FirstOrDefault(p => p.UserId == user.Id);

            if (parentProfile != null)
            {
                var parentStudents = await _unitOfWork.StudentProfiles.GetAllAsync();
                ViewBag.ChildrenCount = 0;
            }
            else
            {
                ViewBag.ChildrenCount = 0;
            }

            return View();
        }
    }
}
