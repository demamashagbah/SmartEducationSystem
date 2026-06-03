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
    public class SubjectController : Controller
    {
        private readonly ISubjectService _subjectService;
        private readonly ITeacherGuideService _teacherGuideService;
        private readonly IWebHostEnvironment _env;

        public SubjectController(ISubjectService subjectService, ITeacherGuideService teacherGuideService, IWebHostEnvironment env)
        {
            _subjectService = subjectService;
            _teacherGuideService = teacherGuideService;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var subjects = await _subjectService.GetAllAsync();
            var viewModels = subjects.Select(s => new SubjectViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description
            });
            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new SubjectViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _subjectService.CreateAsync(new SubjectDto
            {
                Name = model.Name,
                Description = model.Description
            });

            TempData["Success"] = "Subject created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var subject = await _subjectService.GetByIdAsync(id);
            if (subject == null) return NotFound();

            return View(new SubjectViewModel
            {
                Id = subject.Id,
                Name = subject.Name,
                Description = subject.Description
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubjectViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _subjectService.UpdateAsync(new SubjectDto
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description
            });

            TempData["Success"] = "Subject updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _subjectService.DeleteAsync(id);
            TempData["Success"] = "Subject deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var curriculum = await _teacherGuideService.GetSubjectCurriculumAsync(id);
            if (curriculum == null || curriculum.SubjectId == Guid.Empty) return NotFound();
            return View(curriculum);
        }

        [HttpGet]
        public async Task<IActionResult> UploadGuide(Guid id)
        {
            var subject = await _subjectService.GetByIdAsync(id);
            if (subject == null) return NotFound();
            ViewBag.SubjectId = id;
            ViewBag.SubjectName = subject.Name;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadGuide(Guid subjectId, IFormFile file, string? description)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload.");
                ViewBag.SubjectId = subjectId;
                return View();
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "teacher-guides");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var relativePath = $"/uploads/teacher-guides/{uniqueName}";
            await _teacherGuideService.UploadGuideAsync(subjectId, file.FileName, relativePath, description);

            TempData["Success"] = $"Teacher Guide '{file.FileName}' uploaded successfully.";
            return RedirectToAction(nameof(Details), new { id = subjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGuide(Guid guideId, Guid subjectId)
        {
            await _teacherGuideService.DeleteGuideAsync(guideId);
            TempData["Success"] = "Teacher Guide deleted.";
            return RedirectToAction(nameof(Details), new { id = subjectId });
        }
    }
}
