using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Constants;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;
using SmartEducation.Persistence.Contexts;
using SmartEducation.Web.Areas.Admin.Models;

namespace SmartEducation.Web.Areas.Admin.Contollers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class ParentController : Controller
    {
        private readonly IParentManagementService _parentService;
        private readonly IAdminStudentService     _studentService;
        private readonly IUnitOfWork              _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext     _dbContext;

        public ParentController(
            IParentManagementService parentService,
            IAdminStudentService studentService,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext)
        {
            _parentService  = parentService;
            _studentService = studentService;
            _unitOfWork     = unitOfWork;
            _userManager    = userManager;
            _dbContext      = dbContext;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var parents = await _parentService.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.ToLower();
                parents = parents.Where(p =>
                    p.FullName.ToLower().Contains(q) ||
                    p.Email.ToLower().Contains(q) ||
                    (p.PhoneNumber ?? "").ToLower().Contains(q));
            }

            ViewBag.Search = search ?? "";
            return View(parents.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var dto = await _parentService.GetByProfileIdAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var dto = await _parentService.GetByProfileIdAsync(id);
            if (dto == null) return NotFound();
            ViewBag.Children = dto.Children ?? new List<StudentDetailDto>();
            return View(MapToViewModel(dto));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ParentEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, errors) = await _parentService.UpdateParentAsync(new ParentDetailDto
            {
                ProfileId = model.ProfileId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Username = model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                Occupation = model.Occupation,
                EmergencyContact = model.EmergencyContact
            });

            if (!success)
            {
                foreach (var e in errors) ModelState.AddModelError("", e);
                return View(model);
            }

            TempData["Success"] = "Parent updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            await _parentService.DeactivateAsync(id);
            TempData["Success"] = "Parent status updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(Guid id)
        {
            var dto = await _parentService.GetByProfileIdAsync(id);
            if (dto == null) return NotFound();
            ViewBag.ParentName = dto.FullName;
            ViewBag.UserId = dto.UserId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(Guid userId, string newPassword)
        {
            var (success, errors) = await _parentService.ResetPasswordAsync(userId, newPassword);
            TempData[success ? "Success" : "Error"] = success
                ? "Password reset successfully."
                : string.Join(", ", errors);
            return RedirectToAction(nameof(Index));
        }

        // ── AJAX: search students for linking ───────────────────────────────

        [HttpGet]
        public async Task<IActionResult> SearchStudents(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return Json(Array.Empty<object>());

            var students  = await _studentService.GetAllAsync();
            var query     = q.ToLower();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();

            var matched = students
                .Where(s =>
                    s.FullName.ToLower().Contains(query) ||
                    (s.StudentNumber ?? "").ToLower().Contains(query) ||
                    (s.NationalNumber ?? "").ToLower().Contains(query))
                .Take(10)
                .Select(s => new
                {
                    profileId     = s.ProfileId,
                    name          = s.FullName,
                    studentNumber = s.StudentNumber ?? "",
                    className     = s.ClassRoomName ?? "Unassigned"
                });

            return Json(matched);
        }

        // ── AJAX: link student to parent ────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkStudent([FromBody] ParentLinkRequest req)
        {
            var profile = await _unitOfWork.ParentProfiles.GetByIdAsync(req.ParentProfileId);
            if (profile == null) return NotFound(new { error = "Parent not found." });

            var sp = await _unitOfWork.StudentProfiles.GetByIdAsync(req.StudentProfileId);
            if (sp == null) return NotFound(new { error = "Student not found." });

            var exists = _dbContext.ParentStudents
                .Any(ps => ps.ParentId == req.ParentProfileId && ps.StudentId == req.StudentProfileId);
            if (exists) return Ok(new { message = "Already linked." });

            _dbContext.ParentStudents.Add(new ParentStudent
            {
                ParentId  = req.ParentProfileId,
                StudentId = req.StudentProfileId
            });
            await _dbContext.SaveChangesAsync();

            // Get student details for the response
            var studentDto = await _studentService.GetByProfileIdAsync(req.StudentProfileId);
            return Ok(new
            {
                success       = true,
                profileId     = req.StudentProfileId,
                name          = studentDto?.FullName ?? "",
                studentNumber = studentDto?.StudentNumber ?? "",
                className     = studentDto?.ClassRoomName ?? "Unassigned"
            });
        }

        // ── AJAX: unlink student from parent ───────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlinkStudent([FromBody] ParentLinkRequest req)
        {
            var link = _dbContext.ParentStudents
                .FirstOrDefault(ps => ps.ParentId == req.ParentProfileId && ps.StudentId == req.StudentProfileId);

            if (link != null)
            {
                _dbContext.ParentStudents.Remove(link);
                await _dbContext.SaveChangesAsync();
            }
            return Ok(new { success = true });
        }

        // ── ──────────────────────────────────────────────────────────────────

        private static ParentEditViewModel MapToViewModel(ParentDetailDto dto) => new()
        {
            ProfileId = dto.ProfileId,
            UserId = dto.UserId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Username = dto.Username,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            Occupation = dto.Occupation,
            EmergencyContact = dto.EmergencyContact
        };
    }

    public class ParentLinkRequest
    {
        public Guid ParentProfileId  { get; set; }
        public Guid StudentProfileId { get; set; }
    }
}
