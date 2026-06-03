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
            var mySubjectIds = teacherAssignments.Where(ta => ta.TeacherId == profile.Id)
                                                  .Select(ta => ta.SubjectId).Distinct().ToList();
            var myClassIds = teacherAssignments.Where(ta => ta.TeacherId == profile.Id)
                                                .Select(ta => ta.ClassRoomId).Distinct().ToList();

            var allExams = await _examService.GetAllAsync();
            var myExams = allExams.Where(e =>
                mySubjectIds.Contains(e.SubjectId) ||
                (e.ClassRoomId.HasValue && myClassIds.Contains(e.ClassRoomId.Value)))
                .OrderByDescending(e => e.ExamDate).ToList();

            return View(myExams);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            await PopulateDropdowns(profile.Id);
            return View(new ExamDto { ExamDate = DateTime.Today.AddDays(7), TotalMarks = 100, DurationMinutes = 60 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamDto dto)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(profile.Id);
                return View(dto);
            }

            await _examService.CreateAsync(dto);
            TempData["Success"] = "Exam created successfully.";
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
                    StudentExamId = se.Id,
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
            await PopulateTopicDropdownsForExam(profile.Id);
            return View(new ExamDto { ExamDate = DateTime.Today.AddDays(14), TotalMarks = 100, DurationMinutes = 60 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateAI(ExamDto dto, List<Guid> topicIds, int questionCount, string difficulty)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var exam = await _examService.CreateAsync(dto);

            // Generate questions from selected topics
            var topics = await _unitOfWork.Topics.GetAllAsync();
            var outcomes = await _unitOfWork.LearningOutcomes.GetAllAsync();
            var questionTypes = Enum.GetValues<SmartEducation.Domain.Enums.QuestionType>();
            var diffLevel = Enum.TryParse<SmartEducation.Domain.Enums.DifficultyLevel>(difficulty, out var dl) ? dl : SmartEducation.Domain.Enums.DifficultyLevel.Medium;

            int qPerTopic = topicIds.Count > 0 ? Math.Max(1, questionCount / topicIds.Count) : 0;
            int qIndex = 0;

            foreach (var topicId in topicIds.Take(10))
            {
                var topic = topics.FirstOrDefault(t => t.Id == topicId);
                if (topic == null) continue;

                var topicOutcomes = outcomes.Where(lo => lo.TopicId == topicId).ToList();
                int questionsForThisTopic = Math.Min(qPerTopic, questionCount - qIndex);

                for (int q = 0; q < questionsForThisTopic; q++)
                {
                    var qType = questionTypes[qIndex % questionTypes.Length];
                    var question = BuildQuestion(topic.Name, topicOutcomes.Select(lo => lo.Description).ToList(), qType, q + 1, exam.TotalMarks / questionCount);

                    await _unitOfWork.QuestionBanks.AddAsync(question);
                    question.SubjectId = dto.SubjectId;
                    question.ExamId = exam.Id;
                    question.QuestionType = qType;
                    question.DifficultyLevel = diffLevel;

                    qIndex++;
                    if (qIndex >= questionCount) break;
                }
                if (qIndex >= questionCount) break;
            }

            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = $"Exam created with {qIndex} AI-generated questions!";
            return RedirectToAction(nameof(Index));
        }

        private static SmartEducation.Domain.Entities.QuestionBank BuildQuestion(string topicName, List<string> outcomes, SmartEducation.Domain.Enums.QuestionType qType, int num, int marks)
        {
            var outcome = outcomes.FirstOrDefault() ?? topicName;
            var q = new SmartEducation.Domain.Entities.QuestionBank
            {
                Id = Guid.NewGuid(),
                Marks = Math.Max(1, marks)
            };

            switch (qType)
            {
                case SmartEducation.Domain.Enums.QuestionType.MultipleChoice:
                    q.QuestionText = $"Which of the following best describes {topicName}?";
                    q.OptionA = $"The primary concept of {topicName}";
                    q.OptionB = $"An unrelated concept to {topicName}";
                    q.OptionC = $"A partial definition of {topicName}";
                    q.OptionD = $"None of the above";
                    q.CorrectAnswer = "A";
                    break;
                case SmartEducation.Domain.Enums.QuestionType.TrueFalse:
                    q.QuestionText = $"{outcome} — This statement is correct regarding {topicName}.";
                    q.OptionA = "True";
                    q.OptionB = "False";
                    q.CorrectAnswer = "True";
                    break;
                case SmartEducation.Domain.Enums.QuestionType.Essay:
                    q.QuestionText = $"Explain in detail: {outcome}. Use examples related to {topicName} to support your answer.";
                    q.CorrectAnswer = $"Expected answer should demonstrate understanding of {topicName} and cover: {outcome}";
                    break;
                default:
                    q.QuestionText = $"Fill in the blank: ________ is a key concept in {topicName}.";
                    q.CorrectAnswer = topicName;
                    break;
            }
            return q;
        }

        private async Task PopulateTopicDropdownsForExam(Guid teacherProfileId)
        {
            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var mySubjectIds = teacherAssignments.Where(ta => ta.TeacherId == teacherProfileId)
                                                  .Select(ta => ta.SubjectId).Distinct().ToList();
            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var units = await _unitOfWork.Units.GetAllAsync();
            var lessons = await _unitOfWork.Lessons.GetAllAsync();
            var topics = await _unitOfWork.Topics.GetAllAsync();

            var myUnitIds = units.Where(u => mySubjectIds.Contains(u.SubjectId)).Select(u => u.Id).ToList();
            var myLessonIds = lessons.Where(l => myUnitIds.Contains(l.UnitId)).Select(l => l.Id).ToList();
            var myTopics = topics.Where(t => myLessonIds.Contains(t.LessonId)).ToList();

            ViewBag.TopicsList = myTopics.Select(t => {
                var lesson = lessons.FirstOrDefault(l => l.Id == t.LessonId);
                var unit = lesson != null ? units.FirstOrDefault(u => u.Id == lesson.UnitId) : null;
                var subject = unit != null ? subjects.FirstOrDefault(s => s.Id == unit.SubjectId) : null;
                return new { Value = t.Id.ToString(), Text = $"{subject?.Name} › {unit?.Name} › {lesson?.Name} › {t.Name}" };
            }).ToList();

            ViewBag.Difficulties = Enum.GetNames<SmartEducation.Domain.Enums.DifficultyLevel>()
                .Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(d, d));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            await _examService.DeleteAsync(id);
            TempData["Success"] = "Exam deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(Guid teacherProfileId)
        {
            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var myAssignments = teacherAssignments.Where(ta => ta.TeacherId == teacherProfileId).ToList();
            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();

            var mySubjectIds = myAssignments.Select(ta => ta.SubjectId).Distinct().ToList();
            var myClassIds = myAssignments.Select(ta => ta.ClassRoomId).Distinct().ToList();

            ViewBag.Subjects = subjects.Where(s => mySubjectIds.Contains(s.Id))
                .Select(s => new SelectListItem(s.Name, s.Id.ToString()));
            ViewBag.ClassRooms = classRooms.Where(c => myClassIds.Contains(c.Id))
                .Select(c => new SelectListItem(c.Name, c.Id.ToString()));
            ViewBag.ExamTypes = Enum.GetValues<ExamType>()
                .Select(t => new SelectListItem(t.ToString(), ((int)t).ToString()));
        }
    }
}
