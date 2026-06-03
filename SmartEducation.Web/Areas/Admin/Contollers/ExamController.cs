using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartEducation.Application.Constants;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Enums;

namespace SmartEducation.Web.Areas.Admin.Contollers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class ExamController : Controller
    {
        private readonly IExamService _examService;
        private readonly ISubjectService _subjectService;
        private readonly IClassRoomService _classRoomService;

        public ExamController(IExamService examService, ISubjectService subjectService, IClassRoomService classRoomService)
        {
            _examService = examService;
            _subjectService = subjectService;
            _classRoomService = classRoomService;
        }

        public async Task<IActionResult> Index()
        {
            var exams = await _examService.GetAllAsync();
            return View(exams);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View(new ExamDto { ExamDate = DateTime.Today.AddDays(7), TotalMarks = 100, DurationMinutes = 60 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(dto);
            }
            await _examService.CreateAsync(dto);
            TempData["Success"] = "Exam created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var exam = await _examService.GetByIdAsync(id);
            if (exam == null) return NotFound();
            await PopulateDropdowns();
            return View(exam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ExamDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(dto);
            }
            await _examService.UpdateAsync(dto);
            TempData["Success"] = "Exam updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _examService.DeleteAsync(id);
            TempData["Success"] = "Exam deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns()
        {
            var subjects = await _subjectService.GetAllAsync();
            var classRooms = await _classRoomService.GetAllAsync();
            ViewBag.Subjects = subjects.Select(s => new SelectListItem(s.Name, s.Id.ToString()));
            ViewBag.ClassRooms = classRooms.Select(c => new SelectListItem($"{c.GradeName} - {c.Name}", c.Id.ToString()));
            ViewBag.ExamTypes = Enum.GetValues<ExamType>()
                .Select(e => new SelectListItem(e.ToString(), ((int)e).ToString()));
        }
    }
}
