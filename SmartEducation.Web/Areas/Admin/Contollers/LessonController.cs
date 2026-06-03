using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartEducation.Application.Constants;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces.Services;

namespace SmartEducation.Web.Areas.Admin.Contollers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class LessonController : Controller
    {
        private readonly ICurriculumService _curriculumService;

        public LessonController(ICurriculumService curriculumService)
        {
            _curriculumService = curriculumService;
        }

        public async Task<IActionResult> Index(Guid? unitId)
        {
            var lessons = unitId.HasValue
                ? await _curriculumService.GetLessonsByUnitAsync(unitId.Value)
                : await _curriculumService.GetAllLessonsAsync();

            var units = await _curriculumService.GetAllUnitsAsync();
            ViewBag.Units = new SelectList(units, "Id", "Name", unitId);
            ViewBag.CurrentUnitId = unitId;
            return View(lessons);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Units = await GetUnitOptions();
            return View(new LessonDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LessonDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Units = await GetUnitOptions();
                return View(dto);
            }
            await _curriculumService.CreateLessonAsync(dto);
            TempData["Success"] = "Lesson created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var lesson = await _curriculumService.GetLessonByIdAsync(id);
            if (lesson == null) return NotFound();
            ViewBag.Units = await GetUnitOptions();
            return View(lesson);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LessonDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Units = await GetUnitOptions();
                return View(dto);
            }
            await _curriculumService.UpdateLessonAsync(dto);
            TempData["Success"] = "Lesson updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _curriculumService.DeleteLessonAsync(id);
            TempData["Success"] = "Lesson deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> GetUnitOptions()
        {
            var units = await _curriculumService.GetAllUnitsAsync();
            return units.Select(u => new SelectListItem($"{u.SubjectName} > {u.Name}", u.Id.ToString()));
        }
    }
}
