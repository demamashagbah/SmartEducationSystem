using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Constants;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces.Services;

namespace SmartEducation.Web.Areas.Admin.Contollers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class AcademicYearController : Controller
    {
        private readonly IAcademicYearService _academicYearService;

        public AcademicYearController(IAcademicYearService academicYearService)
        {
            _academicYearService = academicYearService;
        }

        public async Task<IActionResult> Index()
        {
            var years = await _academicYearService.GetAllAsync();
            return View(years);
        }

        [HttpGet]
        public IActionResult Create() => View(new AcademicYearDto
        {
            StartDate = new DateTime(DateTime.Now.Year, 9, 1),
            EndDate = new DateTime(DateTime.Now.Year + 1, 6, 30)
        });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AcademicYearDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _academicYearService.CreateAsync(dto);
            TempData["Success"] = "Academic year created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var year = await _academicYearService.GetByIdAsync(id);
            if (year == null) return NotFound();
            return View(year);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AcademicYearDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _academicYearService.UpdateAsync(dto);
            TempData["Success"] = "Academic year updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _academicYearService.DeleteAsync(id);
            TempData["Success"] = "Academic year deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
