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
    public class LessonPlanController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public LessonPlanController(ITeacherService teacherService, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
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

            var plans = await _teacherService.GetLessonPlansAsync(profile.Id);
            return View(plans);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            await PopulateDropdowns(profile.Id);
            return View(new LessonPlanDto { LessonDate = DateTime.Today, DurationMinutes = 45 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LessonPlanDto dto)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(profile.Id);
                return View(dto);
            }

            await _teacherService.CreateLessonPlanAsync(dto);
            TempData["Success"] = "Lesson plan created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var plan = await _teacherService.GetLessonPlanByIdAsync(id, profile.Id);
            if (plan == null) return NotFound();

            await PopulateDropdowns(profile.Id, plan.TeacherAssignmentId);
            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LessonPlanDto dto)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(profile.Id, dto.TeacherAssignmentId);
                return View(dto);
            }

            await _teacherService.UpdateLessonPlanAsync(dto, profile.Id);
            TempData["Success"] = "Lesson plan updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> View(Guid id)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var plan = await _teacherService.GetLessonPlanByIdAsync(id, profile.Id);
            if (plan == null) return NotFound();
            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            await _teacherService.DeleteLessonPlanAsync(id, profile.Id);
            TempData["Success"] = "Lesson plan deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(Guid teacherProfileId, Guid? selectedAssignmentId = null)
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
                    ta.Id.ToString(),
                    ta.Id == selectedAssignmentId
                );
            });

            var lessons = await _unitOfWork.Lessons.GetAllAsync();
            var units = await _unitOfWork.Units.GetAllAsync();
            var mySubjectIds = myAssignments.Select(ta => ta.SubjectId).Distinct().ToList();
            var myUnitIds = units.Where(u => mySubjectIds.Contains(u.SubjectId)).Select(u => u.Id).ToList();
            var myLessons = lessons.Where(l => myUnitIds.Contains(l.UnitId)).ToList();

            ViewBag.Lessons = myLessons.Select(l =>
            {
                var unit = units.FirstOrDefault(u => u.Id == l.UnitId);
                var subject = unit != null ? subjects.FirstOrDefault(s => s.Id == unit.SubjectId) : null;
                return new SelectListItem($"{subject?.Name} › {unit?.Name} › {l.Name}", l.Id.ToString());
            });
        }
    }
}
