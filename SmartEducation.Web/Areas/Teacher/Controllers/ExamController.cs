using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartEducation.Application.Constants;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;
using SmartEducation.Domain.Enums;
using SmartEducation.Web.Areas.Teacher.Models;

namespace SmartEducation.Web.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = Roles.Teacher)]
    public class ExamController : Controller
    {
        private readonly IExamService _examService;
        private readonly ITeacherService _teacherService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExamController(IExamService examService, ITeacherService teacherService,
            IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _examService = examService;
            _teacherService = teacherService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        private async Task<TeacherProfile?> GetTeacherProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            return user != null ? await _teacherService.GetProfileByUserIdAsync(user.Id) : null;
        }

        public async Task<IActionResult> Index()
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var mySubjectIds = teacherAssignments.Where(ta => ta.TeacherId == profile.Id).Select(ta => ta.SubjectId).Distinct().ToList();
            var myClassIds = teacherAssignments.Where(ta => ta.TeacherId == profile.Id).Select(ta => ta.ClassRoomId).Distinct().ToList();

            var allExams = await _examService.GetAllAsync();
            var myExams = allExams
                .Where(e => mySubjectIds.Contains(e.SubjectId) || (e.ClassRoomId.HasValue && myClassIds.Contains(e.ClassRoomId.Value)))
                .OrderByDescending(e => e.ExamDate).ToList();

            return View(myExams);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            await PopulateDropdowns(profile.Id);
            return View(new ExamCreateViewModel { ExamDate = DateTime.Today.AddDays(7) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamCreateViewModel model)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var sections = (model.Sections ?? new List<ExamSectionViewModel>())
                .Where(s => !string.IsNullOrWhiteSpace(s.SectionName) && s.QuestionCount > 0 && s.MarksEach > 0)
                .ToList();

            if (!sections.Any())
            {
                ModelState.AddModelError("", "Please add at least one exam section with a name, question count, and marks.");
                await PopulateDropdowns(profile.Id);
                return View(model);
            }

            int totalMarks = sections.Sum(s => s.QuestionCount * s.MarksEach);
            var sectionSummary = string.Join(" | ", sections.Select(s => $"{s.SectionName}: {s.QuestionCount}×{s.QuestionType}@{s.MarksEach}mk"));

            var dto = new ExamDto
            {
                Title = model.Title,
                Description = string.IsNullOrWhiteSpace(model.Description)
                    ? $"[Sections: {sectionSummary}]"
                    : $"{model.Description}\n[Sections: {sectionSummary}]",
                SubjectId = model.SubjectId,
                ClassRoomId = model.ClassRoomId,
                ExamDate = model.ExamDate,
                DurationMinutes = model.DurationMinutes,
                TotalMarks = totalMarks
            };

            if (Enum.TryParse<ExamType>(model.ExamType, out var examType)) dto.ExamType = examType;

            await _examService.CreateAsync(dto);
            TempData["Success"] = $"Exam '{model.Title}' created — {totalMarks} marks across {sections.Count} section(s).";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var exam = await _examService.GetByIdAsync(id);
            if (exam == null) return NotFound();

            await PopulateDropdowns(profile.Id);
            return View(exam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ExamDto dto)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(profile.Id);
                return View(dto);
            }

            await _examService.UpdateAsync(dto);
            TempData["Success"] = "Exam updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Results(Guid id)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var exam = await _examService.GetByIdAsync(id);
            if (exam == null) return NotFound();

            var studentExams = await _unitOfWork.StudentExams.GetAllAsync();
            var examResults = studentExams.Where(se => se.ExamId == id).ToList();
            var studentProfiles = await _unitOfWork.StudentProfiles.GetAllAsync();
            var allUsers = _userManager.Users.ToList();

            var results = examResults.Select(se =>
            {
                var sp = studentProfiles.FirstOrDefault(p => p.Id == se.StudentId);
                var user = sp != null ? allUsers.FirstOrDefault(u => u.Id == sp.UserId) : null;
                return new
                {
                    StudentName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                    Score = se.Score,
                    IsSubmitted = se.IsSubmitted,
                    SubmittedAt = se.FinishedAt
                };
            }).OrderByDescending(r => r.Score).ToList();

            ViewBag.Exam = exam;
            ViewBag.Results = results;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GenerateAI()
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            await PopulateDropdowns(profile.Id);
            await PopulateCurriculumTree(profile.Id);
            return View(new ExamCreateViewModel { ExamDate = DateTime.Today.AddDays(14) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateAI(ExamCreateViewModel model, List<Guid> topicIds, string difficulty)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var sections = (model.Sections ?? new List<ExamSectionViewModel>())
                .Where(s => !string.IsNullOrWhiteSpace(s.SectionName) && s.QuestionCount > 0 && s.MarksEach > 0)
                .ToList();

            if (!sections.Any())
                sections = new List<ExamSectionViewModel>
                {
                    new() { SectionName = "Section A", QuestionType = "MultipleChoice", QuestionCount = 10, MarksEach = 1 }
                };

            int totalMarks = sections.Sum(s => s.QuestionCount * s.MarksEach);

            var dto = new ExamDto
            {
                Title = model.Title,
                Description = model.Description,
                SubjectId = model.SubjectId,
                ClassRoomId = model.ClassRoomId,
                ExamDate = model.ExamDate,
                DurationMinutes = model.DurationMinutes,
                TotalMarks = totalMarks
            };
            if (Enum.TryParse<ExamType>(model.ExamType, out var examType)) dto.ExamType = examType;

            var exam = await _examService.CreateAsync(dto);

            var topics = await _unitOfWork.Topics.GetAllAsync();
            var outcomes = await _unitOfWork.LearningOutcomes.GetAllAsync();
            var diffLevel = Enum.TryParse<DifficultyLevel>(difficulty, out var dl) ? dl : DifficultyLevel.Medium;

            var selectedTopics = topicIds.Any()
                ? topics.Where(t => topicIds.Contains(t.Id)).ToList()
                : topics.Take(5).ToList();

            int topicIndex = 0;
            int totalGenerated = 0;

            foreach (var section in sections)
            {
                var qType = section.QuestionType switch
                {
                    "TrueFalse" => QuestionType.TrueFalse,
                    "Essay" => QuestionType.Essay,
                    "FillInBlank" or "FillInTheBlank" => QuestionType.FillInTheBlank,
                    _ => QuestionType.MultipleChoice
                };

                for (int q = 0; q < section.QuestionCount; q++)
                {
                    var topic = selectedTopics.Count > 0 ? selectedTopics[topicIndex % selectedTopics.Count] : null;
                    topicIndex++;

                    var topicName = topic?.Name ?? "General";
                    var topicOutcomes = topic != null
                        ? outcomes.Where(lo => lo.TopicId == topic.Id).Select(lo => lo.Description).ToList()
                        : new List<string>();

                    var question = BuildQuestion(topicName, topicOutcomes, qType, q + 1, section.MarksEach, section.SectionName);
                    question.SubjectId = dto.SubjectId;
                    question.ExamId = exam.Id;
                    question.DifficultyLevel = diffLevel;

                    await _unitOfWork.QuestionBanks.AddAsync(question);
                    totalGenerated++;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = $"AI generated {totalGenerated} questions across {sections.Count} section(s). Total marks: {totalMarks}.";
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

        private static QuestionBank BuildQuestion(string topicName, List<string> outcomes, QuestionType qType, int num, int marks, string sectionName)
        {
            var outcome = outcomes.FirstOrDefault() ?? topicName;
            var q = new QuestionBank { Id = Guid.NewGuid(), Marks = Math.Max(1, marks), QuestionType = qType };

            switch (qType)
            {
                case QuestionType.MultipleChoice:
                    q.QuestionText = $"Which of the following best describes: {topicName}?";
                    q.OptionA = $"The primary concept of {topicName}";
                    q.OptionB = "An unrelated concept";
                    q.OptionC = $"A partial aspect of {topicName}";
                    q.OptionD = "None of the above";
                    q.CorrectAnswer = "A";
                    break;
                case QuestionType.TrueFalse:
                    q.QuestionText = $"{outcome} — This statement is correct regarding {topicName}.";
                    q.OptionA = "True"; q.OptionB = "False"; q.CorrectAnswer = "True";
                    break;
                case QuestionType.Essay:
                    q.QuestionText = $"Explain in detail: {outcome}. Use examples related to {topicName}.";
                    q.CorrectAnswer = $"Answer should demonstrate understanding of {topicName}";
                    break;
                default:
                    q.QuestionText = $"________ is a key concept in {topicName}.";
                    q.CorrectAnswer = topicName;
                    break;
            }
            return q;
        }

        private async Task PopulateCurriculumTree(Guid teacherProfileId)
        {
            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var mySubjectIds = teacherAssignments.Where(ta => ta.TeacherId == teacherProfileId).Select(ta => ta.SubjectId).Distinct().ToList();
            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var units = await _unitOfWork.Units.GetAllAsync();
            var lessons = await _unitOfWork.Lessons.GetAllAsync();
            var topics = await _unitOfWork.Topics.GetAllAsync();

            var myUnitIds = units.Where(u => mySubjectIds.Contains(u.SubjectId)).Select(u => u.Id).ToList();
            var myLessonIds = lessons.Where(l => myUnitIds.Contains(l.UnitId)).Select(l => l.Id).ToList();
            var myTopics = topics.Where(t => myLessonIds.Contains(t.LessonId)).ToList();

            ViewBag.TopicsList = myTopics.Select(t =>
            {
                var lesson = lessons.FirstOrDefault(l => l.Id == t.LessonId);
                var unit = lesson != null ? units.FirstOrDefault(u => u.Id == lesson.UnitId) : null;
                var subject = unit != null ? subjects.FirstOrDefault(s => s.Id == unit.SubjectId) : null;
                return new { Value = t.Id.ToString(), Text = $"{subject?.Name} › {unit?.Name} › {lesson?.Name} › {t.Name}" };
            }).ToList();

            ViewBag.Difficulties = Enum.GetNames<DifficultyLevel>().Select(d => new SelectListItem(d, d));
        }

        private async Task PopulateDropdowns(Guid teacherProfileId)
        {
            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var myAssignments = teacherAssignments.Where(ta => ta.TeacherId == teacherProfileId).ToList();
            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();

            var mySubjectIds = myAssignments.Select(ta => ta.SubjectId).Distinct().ToList();
            var myClassIds = myAssignments.Select(ta => ta.ClassRoomId).Distinct().ToList();

            ViewBag.Subjects = subjects.Where(s => mySubjectIds.Contains(s.Id)).Select(s => new SelectListItem(s.Name, s.Id.ToString()));
            ViewBag.ClassRooms = classRooms.Where(c => myClassIds.Contains(c.Id)).Select(c => new SelectListItem(c.Name, c.Id.ToString()));
            ViewBag.ExamTypes = Enum.GetValues<ExamType>().Select(t => new SelectListItem(t.ToString(), t.ToString()));
        }
    }
}
