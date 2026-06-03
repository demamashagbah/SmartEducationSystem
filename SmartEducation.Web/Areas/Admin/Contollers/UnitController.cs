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
    public class UnitController : Controller
    {
        private readonly ICurriculumService _curriculumService;
        private readonly ISubjectService _subjectService;

        public UnitController(ICurriculumService curriculumService, ISubjectService subjectService)
        {
            _curriculumService = curriculumService;
            _subjectService = subjectService;
        }

        public async Task<IActionResult> Index(Guid? subjectId)
        {
            var units = subjectId.HasValue
                ? await _curriculumService.GetUnitsBySubjectAsync(subjectId.Value)
                : await _curriculumService.GetAllUnitsAsync();

            var subjects = await _subjectService.GetAllAsync();
            ViewBag.Subjects = new SelectList(subjects, "Id", "Name", subjectId);
            ViewBag.CurrentSubjectId = subjectId;
            return View(units);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Subjects = await GetSubjectOptions();
            return View(new UnitDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UnitDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Subjects = await GetSubjectOptions();
                return View(dto);
            }
            await _curriculumService.CreateUnitAsync(dto);
            TempData["Success"] = "Unit created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var unit = await _curriculumService.GetUnitByIdAsync(id);
            if (unit == null) return NotFound();
            ViewBag.Subjects = await GetSubjectOptions();
            return View(unit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UnitDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Subjects = await GetSubjectOptions();
                return View(dto);
            }
            await _curriculumService.UpdateUnitAsync(dto);
            TempData["Success"] = "Unit updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _curriculumService.DeleteUnitAsync(id);
            TempData["Success"] = "Unit deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> GetSubjectOptions()
        {
            var subjects = await _subjectService.GetAllAsync();
            return subjects.Select(s => new SelectListItem(s.Name, s.Id.ToString()));
        }
    }
}
