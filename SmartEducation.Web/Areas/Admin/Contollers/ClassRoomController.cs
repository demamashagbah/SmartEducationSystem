using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartEducation.Application.Constants;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;
using SmartEducation.Persistence.Contexts;
using SmartEducation.Web.Areas.Admin.Models;

namespace SmartEducation.Web.Areas.Admin.Contollers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class ClassRoomController : Controller
    {
        private readonly IClassRoomService _classRoomService;
        private readonly IGradeService _gradeService;
        private readonly IAdminStudentService _studentService;
        private readonly ISubjectService _subjectService;
        private readonly ITeacherAssignmentService _teacherAssignmentService;
        private readonly IAcademicYearService _academicYearService;
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public ClassRoomController(
            IClassRoomService classRoomService,
            IGradeService gradeService,
            IAdminStudentService studentService,
            ISubjectService subjectService,
            ITeacherAssignmentService teacherAssignmentService,
            IAcademicYearService academicYearService,
            IUserService userService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext)
        {
            _classRoomService = classRoomService;
            _gradeService = gradeService;
            _studentService = studentService;
            _subjectService = subjectService;
            _teacherAssignmentService = teacherAssignmentService;
            _academicYearService = academicYearService;
            _userService = userService;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        // ── Class CRUD ──────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var classRooms = await _classRoomService.GetAllAsync();
            var viewModels = classRooms.Select(c => new ClassRoomViewModel
            {
                Id = c.Id, Name = c.Name, GradeId = c.GradeId, GradeName = c.GradeName,
                StudentCount = c.StudentCount, SubjectCount = c.SubjectCount,
                TeacherAssignmentCount = c.TeacherAssignmentCount
            });
            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View(new ClassRoomViewModel { GradeOptions = await GetGradeOptions() });
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
            await _classRoomService.CreateAsync(new ClassRoomDto { Name = model.Name, GradeId = model.GradeId });
            TempData["Success"] = "Class created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var c = await _classRoomService.GetByIdAsync(id);
            if (c == null) return NotFound();
            return View(new ClassRoomViewModel
            {
                Id = c.Id, Name = c.Name, GradeId = c.GradeId,
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
            await _classRoomService.UpdateAsync(new ClassRoomDto { Id = model.Id, Name = model.Name, GradeId = model.GradeId });
            TempData["Success"] = "Class updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _classRoomService.DeleteAsync(id);
            TempData["Success"] = "Class deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── Class Management Hub ─────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Manage(Guid id)
        {
            var classRoom = await _classRoomService.GetByIdAsync(id);
            if (classRoom == null) return NotFound();

            var students = await _studentService.GetByClassRoomAsync(id);
            var subjects = await _subjectService.GetByClassRoomAsync(id);
            var assignments = await _teacherAssignmentService.GetByClassRoomAsync(id);

            ViewBag.ClassRoom = classRoom;
            ViewBag.Students = students.ToList();
            ViewBag.Subjects = subjects.ToList();
            ViewBag.Assignments = assignments.ToList();
            return View();
        }

        // ── Student Assignment (search existing → assign) ────────────────

        [HttpGet]
        public async Task<IActionResult> AssignStudents(Guid id, string? search)
        {
            var classRoom = await _classRoomService.GetByIdAsync(id);
            if (classRoom == null) return NotFound();

            var available = await _studentService.GetAvailableForClassAsync(id);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.ToLower();
                available = available.Where(s =>
                    s.FullName.ToLower().Contains(q) ||
                    s.StudentNumber.ToLower().Contains(q) ||
                    s.NationalNumber.ToLower().Contains(q) ||
                    s.Email.ToLower().Contains(q));
            }

            ViewBag.ClassRoom = classRoom;
            ViewBag.Search = search ?? "";
            return View(available.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignStudents(Guid classRoomId, List<Guid> selectedProfiles)
        {
            if (selectedProfiles == null || !selectedProfiles.Any())
            {
                TempData["Error"] = "Please select at least one student.";
                return RedirectToAction(nameof(AssignStudents), new { id = classRoomId });
            }

            foreach (var profileId in selectedProfiles)
                await _studentService.AssignToClassAsync(profileId, classRoomId);

            TempData["Success"] = $"{selectedProfiles.Count} student(s) assigned to class.";
            return RedirectToAction(nameof(Manage), new { id = classRoomId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStudent(Guid profileId, Guid classRoomId)
        {
            await _studentService.RemoveFromClassAsync(profileId);
            TempData["Success"] = "Student removed from class.";
            return RedirectToAction(nameof(Manage), new { id = classRoomId });
        }

        [HttpGet]
        public async Task<IActionResult> TransferStudent(Guid profileId)
        {
            var dto = await _studentService.GetByProfileIdAsync(profileId);
            if (dto == null) return NotFound();
            ViewBag.Student = dto;
            ViewBag.ClassRoomOptions = await GetClassRoomOptions(exclude: dto.ClassRoomId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferStudent(Guid profileId, Guid newClassRoomId, Guid currentClassRoomId)
        {
            await _studentService.TransferStudentAsync(profileId, newClassRoomId);
            TempData["Success"] = "Student transferred successfully.";
            return RedirectToAction(nameof(Manage), new { id = currentClassRoomId });
        }

        // ── Subject Assignment ────────────────────────────────────────────

        [HttpGet]
        public IActionResult AssignSubject(Guid classRoomId)
        {
            // Redirect to the unified Subject/Create workflow which requires a Teacher Guide PDF
            return RedirectToAction("Create", "Subject", new { area = "Admin", classRoomId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSubject(Guid subjectId, Guid classRoomId)
        {
            await _subjectService.DeleteAsync(subjectId);
            TempData["Success"] = "Subject removed.";
            return RedirectToAction(nameof(Manage), new { id = classRoomId });
        }

        // ── Add New Student (create + immediately assign to class) ─────────

        [HttpGet]
        public async Task<IActionResult> AddStudent(Guid classRoomId)
        {
            var classRoom = await _classRoomService.GetByIdAsync(classRoomId);
            if (classRoom == null) return NotFound();

            return View(new StudentViewModel
            {
                ClassRoomId   = classRoomId,
                ClassRoomName = classRoom.Name,
                EnrollmentDate = DateTime.Today,
                AcademicYearOptions = await GetAcademicYearOptions()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(StudentViewModel model)
        {
            // Remove validation for fields not required when linking an existing parent
            if (model.LinkedParentProfileId.HasValue)
            {
                ModelState.Remove(nameof(model.ParentName));
                ModelState.Remove(nameof(model.ParentPhone));
                ModelState.Remove(nameof(model.ParentEmail));
            }

            if (!ModelState.IsValid)
            {
                model.AcademicYearOptions = await GetAcademicYearOptions();
                var cr = await _classRoomService.GetByIdAsync(model.ClassRoomId);
                model.ClassRoomName = cr?.Name ?? "";
                return View(model);
            }

            var (success, errors) = await _userService.CreateFullUserAsync(new CreateUserFullDto
            {
                FirstName      = model.FirstName,
                LastName       = model.LastName,
                Username       = model.Username,
                Email          = model.Email,
                Gender         = model.Gender,
                DateOfBirth    = model.DateOfBirth,
                Role           = Roles.Student,
                Password       = model.Password ?? "Student@123",
                StudentNumber  = model.StudentNumber,
                NationalNumber = model.NationalNumber,
                AcademicYearId = model.AcademicYearId,
                EnrollmentDate = model.EnrollmentDate,
                ParentName     = model.LinkedParentProfileId.HasValue ? (model.LinkedParentName ?? "") : model.ParentName,
                ParentPhone    = model.LinkedParentProfileId.HasValue ? "" : model.ParentPhone,
                ParentEmail    = model.LinkedParentProfileId.HasValue ? "" : model.ParentEmail,
                Address        = model.Address,
                EmergencyContact = model.EmergencyContact
            });

            if (!success)
            {
                foreach (var e in errors) ModelState.AddModelError("", e);
                model.AcademicYearOptions = await GetAcademicYearOptions();
                var cr2 = await _classRoomService.GetByIdAsync(model.ClassRoomId);
                model.ClassRoomName = cr2?.Name ?? "";
                return View(model);
            }

            // Assign to class
            var newUser = await _userManager.FindByEmailAsync(model.Email);
            if (newUser != null)
            {
                var allProfiles = await _studentService.GetAllAsync();
                var sp = allProfiles.FirstOrDefault(s => s.UserId == newUser.Id);
                if (sp != null)
                {
                    await _studentService.AssignToClassAsync(sp.ProfileId, model.ClassRoomId);

                    // Link to existing parent if selected
                    if (model.LinkedParentProfileId.HasValue)
                    {
                        var alreadyLinked = _dbContext.ParentStudents
                            .Any(ps => ps.ParentId == model.LinkedParentProfileId.Value && ps.StudentId == sp.ProfileId);
                        if (!alreadyLinked)
                        {
                            _dbContext.ParentStudents.Add(new ParentStudent
                            {
                                ParentId  = model.LinkedParentProfileId.Value,
                                StudentId = sp.ProfileId
                            });
                            await _dbContext.SaveChangesAsync();
                        }
                    }
                }
            }

            TempData["Success"] = $"Student {model.FirstName} {model.LastName} added successfully.";
            return RedirectToAction(nameof(Manage), new { id = model.ClassRoomId });
        }

        // ── AJAX: search existing parent accounts ───────────────────────────

        [HttpGet]
        public async Task<IActionResult> SearchParents(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return Json(Array.Empty<object>());

            var allParents = await _userManager.GetUsersInRoleAsync(Roles.Parent);
            var query = q.ToLower();

            var matched = allParents
                .Where(u =>
                    $"{u.FirstName} {u.LastName}".ToLower().Contains(query) ||
                    (u.Email ?? "").ToLower().Contains(query) ||
                    (u.PhoneNumber ?? "").Contains(query))
                .Take(10)
                .ToList();

            var result = new List<object>();
            foreach (var u in matched)
            {
                var profiles = _dbContext.ParentProfiles.Where(p => p.UserId == u.Id).ToList();
                var profile  = profiles.FirstOrDefault();
                if (profile == null) continue;
                result.Add(new
                {
                    profileId = profile.Id,
                    userId    = u.Id,
                    name      = $"{u.FirstName} {u.LastName}".Trim(),
                    email     = u.Email ?? "",
                    phone     = u.PhoneNumber ?? ""
                });
            }
            return Json(result);
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private async Task<IEnumerable<SelectListItem>> GetAcademicYearOptions()
        {
            var years = await _academicYearService.GetAllAsync();
            return years.Select(y => new SelectListItem(y.Name, y.Id.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> GetGradeOptions()
        {
            var grades = await _gradeService.GetAllAsync();
            return grades.Select(g => new SelectListItem(g.Name, g.Id.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> GetClassRoomOptions(Guid? exclude = null)
        {
            var rooms = await _classRoomService.GetAllAsync();
            return rooms
                .Where(r => r.Id != exclude)
                .Select(r => new SelectListItem($"{r.Name} ({r.GradeName})", r.Id.ToString()));
        }
    }
}
