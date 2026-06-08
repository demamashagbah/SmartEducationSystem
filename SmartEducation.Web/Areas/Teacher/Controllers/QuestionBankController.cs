using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartEducation.Application.Constants;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;
using SmartEducation.Domain.Enums;

namespace SmartEducation.Web.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = Roles.Teacher)]
    public class QuestionBankController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public QuestionBankController(ITeacherService teacherService, IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _teacherService = teacherService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        private async Task<TeacherProfile?> GetTeacherProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            return user != null ? await _teacherService.GetProfileByUserIdAsync(user.Id) : null;
        }

        public async Task<IActionResult> Index(Guid? subjectId, string? questionType, string? difficulty)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var mySubjectIds = teacherAssignments.Where(ta => ta.TeacherId == profile.Id)
                .Select(ta => ta.SubjectId).Distinct().ToList();

            var allQuestions = await _unitOfWork.QuestionBanks.GetAllAsync();
            var myQuestions = allQuestions.Where(q => mySubjectIds.Contains(q.SubjectId)).ToList();

            if (subjectId.HasValue)
                myQuestions = myQuestions.Where(q => q.SubjectId == subjectId.Value).ToList();
            if (!string.IsNullOrEmpty(questionType) && Enum.TryParse<QuestionType>(questionType, out var qt))
                myQuestions = myQuestions.Where(q => q.QuestionType == qt).ToList();
            if (!string.IsNullOrEmpty(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, out var dl))
                myQuestions = myQuestions.Where(q => q.DifficultyLevel == dl).ToList();

            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var mySubjects = subjects.Where(s => mySubjectIds.Contains(s.Id)).ToList();

            ViewBag.SubjectFilter   = subjectId;
            ViewBag.QuestionTypeFilter = questionType ?? "";
            ViewBag.DifficultyFilter   = difficulty ?? "";
            ViewBag.Subjects    = mySubjects.Select(s => new SelectListItem(s.Name, s.Id.ToString()));
            ViewBag.QuestionTypes = Enum.GetValues<QuestionType>().Select(t => new SelectListItem(t.ToString(), t.ToString()));
            ViewBag.Difficulties  = Enum.GetValues<DifficultyLevel>().Select(d => new SelectListItem(d.ToString(), d.ToString()));

            var subjectDict = mySubjects.ToDictionary(s => s.Id, s => s.Name);
            ViewBag.SubjectDict = subjectDict;

            return View(myQuestions.OrderBy(q => q.SubjectId).ThenBy(q => q.QuestionType).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");
            await PopulateDropdowns(profile.Id);
            return View(new QuestionBankFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionBankFormViewModel model)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(profile.Id);
                return View(model);
            }

            var entity = new QuestionBank
            {
                Id             = Guid.NewGuid(),
                QuestionText   = model.QuestionText,
                SubjectId      = model.SubjectId,
                QuestionType   = Enum.Parse<QuestionType>(model.QuestionType),
                DifficultyLevel = Enum.Parse<DifficultyLevel>(model.DifficultyLevel),
                OptionA        = model.OptionA,
                OptionB        = model.OptionB,
                OptionC        = model.OptionC,
                OptionD        = model.OptionD,
                CorrectAnswer  = model.CorrectAnswer,
                Marks          = model.Marks
            };

            await _unitOfWork.QuestionBanks.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Question added to bank.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _unitOfWork.QuestionBanks.GetByIdAsync(id);
            if (entity != null)
            {
                await _unitOfWork.QuestionBanks.DeleteAsync(entity);
                await _unitOfWork.SaveChangesAsync();
            }
            TempData["Success"] = "Question deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(Guid teacherProfileId)
        {
            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var mySubjectIds = teacherAssignments.Where(ta => ta.TeacherId == teacherProfileId)
                .Select(ta => ta.SubjectId).Distinct().ToList();
            var subjects = await _unitOfWork.Subjects.GetAllAsync();

            ViewBag.Subjects      = subjects.Where(s => mySubjectIds.Contains(s.Id))
                .Select(s => new SelectListItem(s.Name, s.Id.ToString()));
            ViewBag.QuestionTypes = Enum.GetValues<QuestionType>().Select(t => new SelectListItem(t.ToString(), t.ToString()));
            ViewBag.Difficulties  = Enum.GetValues<DifficultyLevel>().Select(d => new SelectListItem(d.ToString(), d.ToString()));
        }
    }

    public class QuestionBankFormViewModel
    {
        public string QuestionText  { get; set; } = string.Empty;
        public Guid SubjectId       { get; set; }
        public string QuestionType  { get; set; } = "MultipleChoice";
        public string DifficultyLevel { get; set; } = "Medium";
        public string? OptionA      { get; set; }
        public string? OptionB      { get; set; }
        public string? OptionC      { get; set; }
        public string? OptionD      { get; set; }
        public string? CorrectAnswer { get; set; }
        public int Marks            { get; set; } = 1;
    }
}
