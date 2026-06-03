using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Constants;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Web.Areas.Admin.Models;

namespace SmartEducation.Web.Areas.Admin.Contollers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class GradeController : Controller
    {
        private readonly IGradeService _gradeService;

        public GradeController(IGradeService gradeService)
        {
            _gradeService = gradeService;
        }

        public async Task<IActionResult> Index()
        {
            var grades = await _gradeService.GetAllAsync();
            var viewModels = grades.Select(g => new GradeViewModel
            {
                Id = g.Id,
                Name = g.Name,
                ClassRoomCount = g.ClassRoomCount
            });
            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create() => View(new GradeViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GradeViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            await _gradeService.CreateAsync(new GradeDto { Name = model.Name });
            TempData["Success"] = "Grade created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var grade = await _gradeService.GetByIdAsync(id);
            if (grade == null) return NotFound();
            return View(new GradeViewModel { Id = grade.Id, Name = grade.Name });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GradeViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            await _gradeService.UpdateAsync(new GradeDto { Id = model.Id, Name = model.Name });
            TempData["Success"] = "Grade updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _gradeService.DeleteAsync(id);
            TempData["Success"] = "Grade deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
