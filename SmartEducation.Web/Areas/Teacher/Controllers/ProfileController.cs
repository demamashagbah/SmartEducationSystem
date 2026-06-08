using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Constants;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Web.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = Roles.Teacher)]
    public class ProfileController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(ITeacherService teacherService, IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _teacherService = teacherService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

            var profile = await _teacherService.GetProfileByUserIdAsync(user.Id);

            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var myAssignments = profile != null
                ? teacherAssignments.Where(ta => ta.TeacherId == profile.Id).ToList()
                : new List<TeacherAssignment>();

            var subjects   = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();

            var vm = new TeacherProfileViewModel
            {
                UserId            = user.Id,
                FirstName         = user.FirstName,
                LastName          = user.LastName,
                Email             = user.Email ?? "",
                EmployeeNumber    = profile?.EmployeeNumber ?? "N/A",
                Specialization    = profile?.Specialization,
                Qualification     = profile?.Qualification,
                YearsOfExperience = profile?.YearsOfExperience,
                AssignedSubjects  = myAssignments.Select(ta => ta.SubjectId).Distinct()
                    .Select(sid => subjects.FirstOrDefault(s => s.Id == sid)?.Name ?? "")
                    .Where(n => !string.IsNullOrEmpty(n)).ToList(),
                AssignedClasses   = myAssignments.Select(ta => ta.ClassRoomId).Distinct()
                    .Select(cid => classRooms.FirstOrDefault(c => c.Id == cid)?.Name ?? "")
                    .Where(n => !string.IsNullOrEmpty(n)).ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

            var profile = await _teacherService.GetProfileByUserIdAsync(user.Id);
            var vm = new TeacherProfileEditViewModel
            {
                FirstName         = user.FirstName,
                LastName          = user.LastName,
                Specialization    = profile?.Specialization,
                Qualification     = profile?.Qualification,
                YearsOfExperience = profile?.YearsOfExperience
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TeacherProfileEditViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Index));

            user.FirstName = vm.FirstName;
            user.LastName  = vm.LastName;
            await _userManager.UpdateAsync(user);

            var profile = await _teacherService.GetProfileByUserIdAsync(user.Id);
            if (profile != null)
            {
                profile.Specialization    = vm.Specialization;
                profile.Qualification     = vm.Qualification;
                profile.YearsOfExperience = vm.YearsOfExperience;
                await _unitOfWork.TeacherProfiles.UpdateAsync(profile);
                await _unitOfWork.SaveChangesAsync();
            }

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }

    public class TeacherProfileViewModel
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName  { get; set; } = default!;
        public string Email     { get; set; } = default!;
        public string EmployeeNumber  { get; set; } = default!;
        public string? Specialization { get; set; }
        public string? Qualification  { get; set; }
        public int? YearsOfExperience { get; set; }
        public List<string> AssignedSubjects { get; set; } = new();
        public List<string> AssignedClasses  { get; set; } = new();
        public string FullName => $"{FirstName} {LastName}";
    }

    public class TeacherProfileEditViewModel
    {
        public string FirstName { get; set; } = default!;
        public string LastName  { get; set; } = default!;
        public string? Specialization { get; set; }
        public string? Qualification  { get; set; }
        public int? YearsOfExperience { get; set; }
    }
}
