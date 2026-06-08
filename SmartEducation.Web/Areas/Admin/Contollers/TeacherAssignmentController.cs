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
    public class TeacherAssignmentController : Controller
    {
        private readonly ITeacherAssignmentService _assignmentService;
        private readonly IUserService _userService;
        private readonly ISubjectService _subjectService;
        private readonly IClassRoomService _classRoomService;
        private readonly IAcademicYearService _academicYearService;

        public TeacherAssignmentController(
            ITeacherAssignmentService assignmentService,
            IUserService userService,
            ISubjectService subjectService,
            IClassRoomService classRoomService,
            IAcademicYearService academicYearService)
        {
            _assignmentService = assignmentService;
            _userService = userService;
            _subjectService = subjectService;
            _classRoomService = classRoomService;
            _academicYearService = academicYearService;
        }

        public async Task<IActionResult> Index()
        {
            var assignments = await _assignmentService.GetAllAsync();
            return View(assignments);
        }

        [HttpGet]
        public async Task<IActionResult> Assign(Guid? classRoomId = null)
        {
            var model = new TeacherAssignmentViewModel
            {
                ClassRoomId = classRoomId ?? Guid.Empty,
                TeacherOptions = await GetTeacherOptions(),
                ClassRoomOptions = await GetClassRoomOptions(),
                AcademicYearOptions = await GetAcademicYearOptions()
            };

            if (classRoomId.HasValue)
                model.SubjectOptions = await GetSubjectOptions(classRoomId.Value);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(TeacherAssignmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateOptions(model);
                return View(model);
            }

            var (success, error) = await _assignmentService.AssignTeacherAsync(new TeacherAssignmentDetailDto
            {
                TeacherId = model.TeacherId,
                SubjectId = model.SubjectId,
                ClassRoomId = model.ClassRoomId,
                AcademicYearId = model.AcademicYearId
            });

            if (!success)
            {
                ModelState.AddModelError("", error);
                await PopulateOptions(model);
                return View(model);
            }

            TempData["Success"] = "Teacher assigned successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(Guid id)
        {
            await _assignmentService.RemoveAssignmentAsync(id);
            TempData["Success"] = "Assignment removed.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            await _assignmentService.ToggleActiveAsync(id);
            TempData["Success"] = "Assignment status updated.";
            return RedirectToAction(nameof(Index));
        }

        // AJAX: return subjects for selected classroom
        [HttpGet]
        public async Task<IActionResult> GetSubjectsForClass(Guid classRoomId)
        {
            var subjects = await _subjectService.GetByClassRoomAsync(classRoomId);
            return Json(subjects.Select(s => new { value = s.Id, text = s.Name }));
        }

        private async Task PopulateOptions(TeacherAssignmentViewModel model)
        {
            model.TeacherOptions = await GetTeacherOptions();
            model.ClassRoomOptions = await GetClassRoomOptions();
            model.AcademicYearOptions = await GetAcademicYearOptions();
            if (model.ClassRoomId != Guid.Empty)
                model.SubjectOptions = await GetSubjectOptions(model.ClassRoomId);
        }

        private async Task<IEnumerable<SelectListItem>> GetTeacherOptions()
        {
            var teachers = await _userService.GetByRoleAsync(Roles.Teacher);
            return teachers.Select(t => new SelectListItem(t.FullName, t.Id.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> GetClassRoomOptions()
        {
            var rooms = await _classRoomService.GetAllAsync();
            return rooms.Select(r => new SelectListItem($"{r.Name} ({r.GradeName})", r.Id.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> GetSubjectOptions(Guid classRoomId)
        {
            var subjects = await _subjectService.GetByClassRoomAsync(classRoomId);
            return subjects.Select(s => new SelectListItem(s.Name, s.Id.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> GetAcademicYearOptions()
        {
            var years = await _academicYearService.GetAllAsync();
            return years.Select(y => new SelectListItem(y.Name, y.Id.ToString()));
        }
    }
}
