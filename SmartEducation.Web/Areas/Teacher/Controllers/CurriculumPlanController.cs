using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartEducation.Application.Constants;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Web.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = Roles.Teacher)]
    public class CurriculumPlanController : Controller
    {
        private readonly ICurriculumPlanService _planService;
        private readonly ITeacherService _teacherService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public CurriculumPlanController(ICurriculumPlanService planService, ITeacherService teacherService,
            IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _planService = planService;
            _teacherService = teacherService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        private async Task<TeacherProfile?> GetTeacherProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            return user != null ? await _teacherService.GetProfileByUserIdAsync(user.Id) : null;
        }

        public async Task<IActionResult> Index()
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var plans = await _planService.GetByTeacherAsync(profile.Id);
            return View(plans);
        }

        [HttpGet]
        public async Task<IActionResult> Generate()
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            await PopulateDropdowns(profile.Id);
            return View(new GenerateCurriculumPlanRequest
            {
                StartDate = DateTime.Today,
                PlanType = "Weekly",
                LessonsPerWeek = 2
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(GenerateCurriculumPlanRequest request)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(profile.Id);
                return View(request);
            }

            try
            {
                var plan = await _planService.GeneratePlanAsync(request);
                TempData["Success"] = $"Curriculum plan '{plan.Title}' generated with {plan.TotalItems} lesson items!";
                return RedirectToAction(nameof(View), new { id = plan.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error generating plan: {ex.Message}");
                await PopulateDropdowns(profile.Id);
                return View(request);
            }
        }

        [HttpGet]
        public async Task<IActionResult> View(Guid id)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var plan = await _planService.GetByIdAsync(id);
            if (plan == null) return NotFound();

            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleComplete(Guid itemId, Guid planId, bool completed)
        {
            await _planService.MarkItemCompletedAsync(itemId, completed);
            return RedirectToAction(nameof(View), new { id = planId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _planService.DeletePlanAsync(id);
            TempData["Success"] = "Curriculum plan deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(Guid teacherProfileId)
        {
            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var myAssignments = teacherAssignments.Where(ta => ta.TeacherId == teacherProfileId).ToList();
            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();

            ViewBag.TeacherAssignments = myAssignments.Select(ta =>
            {
                var subject = subjects.FirstOrDefault(s => s.Id == ta.SubjectId);
                var classRoom = classRooms.FirstOrDefault(c => c.Id == ta.ClassRoomId);
                return new SelectListItem(
                    $"{subject?.Name ?? "?"} — {classRoom?.Name ?? "?"}",
                    ta.Id.ToString()
                );
            });

            ViewBag.PlanTypes = new List<SelectListItem>
            {
                new("Weekly Plan (1 week)", "Weekly"),
                new("Monthly Plan (1 month)", "Monthly"),
                new("Semester Plan (4 months)", "Semester")
            };
        }
    }
}
