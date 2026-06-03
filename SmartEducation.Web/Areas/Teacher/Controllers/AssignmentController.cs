using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartEducation.Application.Constants;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Web.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = Roles.Teacher)]
    public class AssignmentController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public AssignmentController(ITeacherService teacherService, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
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

        public async Task<IActionResult> Index()
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var assignments = await _teacherService.GetMyAssignmentsAsync(profile.Id);
            return View(assignments);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");
            await PopulateDropdowns(profile.Id);
            return View(new AssignmentDto { DueDate = DateTime.Today.AddDays(7), MaxScore = 100 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssignmentDto dto)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(profile.Id);
                return View(dto);
            }

            await _teacherService.CreateAssignmentAsync(dto, profile.Id);
            TempData["Success"] = "Assignment created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GenerateAI()
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            await PopulateDropdowns(profile.Id);
            await PopulateTopicDropdowns(profile.Id);
            return View(new AssignmentDto { DueDate = DateTime.Today.AddDays(7), MaxScore = 100 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateAI(AssignmentDto dto, Guid topicId)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            // AI generate description from topic + learning outcomes
            if (topicId != Guid.Empty)
            {
                var topics = await _unitOfWork.Topics.GetAllAsync();
                var outcomes = await _unitOfWork.LearningOutcomes.GetAllAsync();
                var topic = topics.FirstOrDefault(t => t.Id == topicId);
                var topicOutcomes = outcomes.Where(lo => lo.TopicId == topicId)
                                            .Select(lo => lo.Description).ToList();

                if (topic != null)
                {
                    dto.Title = string.IsNullOrEmpty(dto.Title) ? $"Assignment: {topic.Name}" : dto.Title;
                    dto.Description = GenerateAssignmentDescription(topic.Name, topicOutcomes);
                }
            }

            await _teacherService.CreateAssignmentAsync(dto, profile.Id);
            TempData["Success"] = "AI assignment generated and saved successfully.";
            return RedirectToAction(nameof(Index));
        }

        private static string GenerateAssignmentDescription(string topicName, List<string> outcomes)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"**Topic:** {topicName}");
            sb.AppendLine();
            sb.AppendLine("**Instructions:**");
            sb.AppendLine($"1. Read the course material on {topicName} carefully.");
            sb.AppendLine("2. Answer all questions in the sections below.");
            sb.AppendLine("3. Show your working and reasoning for each answer.");
            sb.AppendLine();
            if (outcomes.Any())
            {
                sb.AppendLine("**Learning Outcomes to Demonstrate:**");
                foreach (var outcome in outcomes.Take(5))
                    sb.AppendLine($"• {outcome}");
                sb.AppendLine();
            }
            sb.AppendLine("**Questions:**");
            sb.AppendLine($"Q1. Explain the main concepts related to {topicName} in your own words. (20 marks)");
            sb.AppendLine($"Q2. Provide a real-world example that demonstrates your understanding of {topicName}. (20 marks)");
            sb.AppendLine($"Q3. Analyze and evaluate how {topicName} applies in a practical context. (30 marks)");
            sb.AppendLine("Q4. Create a summary diagram or mind map of the key ideas. (30 marks)");
            return sb.ToString().Trim();
        }

        [HttpGet]
        public async Task<IActionResult> Submissions(Guid id)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var assignment = await _unitOfWork.Assignments.GetByIdAsync(id);
            if (assignment == null || assignment.TeacherId != profile.Id) return NotFound();

            var submissions = await _unitOfWork.AssignmentSubmissions.GetAllAsync();
            var assignmentSubmissions = submissions.Where(s => s.AssignmentId == id).ToList();
            var studentProfiles = await _unitOfWork.StudentProfiles.GetAllAsync();
            var allUsers = _userManager.Users.ToList();
            var subjects = await _unitOfWork.Subjects.GetAllAsync();

            ViewBag.Assignment = new AssignmentDto
            {
                Id = assignment.Id,
                Title = assignment.Title,
                SubjectName = subjects.FirstOrDefault(s => s.Id == assignment.SubjectId)?.Name ?? "",
                DueDate = assignment.DueDate,
                MaxScore = assignment.MaxScore
            };

            var submissionList = assignmentSubmissions.Select(s =>
            {
                var sp = studentProfiles.FirstOrDefault(p => p.Id == s.StudentId);
                var user = sp != null ? allUsers.FirstOrDefault(u => u.Id == sp.UserId) : null;
                return new
                {
                    SubmissionId = s.Id,
                    StudentName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                    Notes = s.Notes,
                    SubmittedAt = s.SubmittedAt,
                    Grade = s.Grade,
                    Feedback = s.Feedback
                };
            }).ToList();

            ViewBag.Submissions = submissionList;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeSubmission(Guid submissionId, double grade, string? feedback)
        {
            var submission = await _unitOfWork.AssignmentSubmissions.GetByIdAsync(submissionId);
            if (submission == null) return NotFound();

            submission.Grade = grade;
            submission.Feedback = feedback;
            await _unitOfWork.AssignmentSubmissions.UpdateAsync(submission);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Grade saved.";
            return RedirectToAction(nameof(Submissions), new { id = submission.AssignmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            await _teacherService.DeleteAssignmentAsync(id, profile.Id);
            TempData["Success"] = "Assignment deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateTopicDropdowns(Guid teacherProfileId)
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

            ViewBag.Topics = myTopics.Select(t =>
            {
                var lesson = lessons.FirstOrDefault(l => l.Id == t.LessonId);
                var unit = lesson != null ? units.FirstOrDefault(u => u.Id == lesson.UnitId) : null;
                var subject = unit != null ? subjects.FirstOrDefault(s => s.Id == unit.SubjectId) : null;
                return new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(
                    $"{subject?.Name} › {unit?.Name} › {lesson?.Name} › {t.Name}", t.Id.ToString());
            });
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
        }
    }
}
