using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Domain.Entities;
using SmartEducation.Web.Models;

namespace SmartEducation.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;

            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password");

                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                false,
                false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid email or password");

                return View(model);
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard",
                    new { area = "Admin" });
            }

            if (await _userManager.IsInRoleAsync(user, "Teacher"))
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard",
                    new { area = "Teacher" });
            }

            if (await _userManager.IsInRoleAsync(user, "Student"))
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard",
                    new { area = "Student" });
            }

            if (await _userManager.IsInRoleAsync(user, "Parent"))
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard",
                    new { area = "Parent" });
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
