using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartEducation.Application.Constants;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Web.Areas.Admin.Models;

namespace SmartEducation.Web.Areas.Admin.Contollers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class ClassRoomController : Controller
    {
        private readonly IClassRoomService _classRoomService;
        private readonly IGradeService _gradeService;

        public ClassRoomController(IClassRoomService classRoomService, IGradeService gradeService)
        {
            _classRoomService = classRoomService;
            _gradeService = gradeService;
        }

        public async Task<IActionResult> Index()
        {
            var classRooms = await _classRoomService.GetAllAsync();
            var viewModels = classRooms.Select(c => new ClassRoomViewModel
            {
                Id = c.Id,
                Name = c.Name,
                GradeId = c.GradeId,
                GradeName = c.GradeName,
                StudentCount = c.StudentCount
            });
            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new ClassRoomViewModel
            {
                GradeOptions = await GetGradeOptions()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassRoomViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.GradeOptions = await GetGradeOptions();
                return View(model);
            }

            await _classRoomService.CreateAsync(new ClassRoomDto
            {
                Name = model.Name,
                GradeId = model.GradeId
            });

            TempData["Success"] = "Classroom created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var c = await _classRoomService.GetByIdAsync(id);
            if (c == null) return NotFound();

            return View(new ClassRoomViewModel
            {
                Id = c.Id,
                Name = c.Name,
                GradeId = c.GradeId,
                GradeOptions = await GetGradeOptions()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClassRoomViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.GradeOptions = await GetGradeOptions();
                return View(model);
            }

            await _classRoomService.UpdateAsync(new ClassRoomDto
            {
                Id = model.Id,
                Name = model.Name,
                GradeId = model.GradeId
            });

            TempData["Success"] = "Classroom updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _classRoomService.DeleteAsync(id);
            TempData["Success"] = "Classroom deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> GetGradeOptions()
        {
            var grades = await _gradeService.GetAllAsync();
            return grades.Select(g => new SelectListItem(g.Name, g.Id.ToString()));
        }
    }
}
