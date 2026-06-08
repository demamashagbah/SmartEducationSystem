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
    public class SubjectsController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubjectsController(ITeacherService teacherService, IUnitOfWork unitOfWork,
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

        public async Task<IActionResult> Index()
        {
            var profile = await GetTeacherProfile();
            if (profile == null) return RedirectToAction("Index", "Dashboard");

            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var myAssignments = teacherAssignments.Where(ta => ta.TeacherId == profile.Id).ToList();
            var mySubjectIds = myAssignments.Select(ta => ta.SubjectId).Distinct().ToList();

            var subjects  = await _unitOfWork.Subjects.GetAllAsync();
            var units     = await _unitOfWork.Units.GetAllAsync();
            var lessons   = await _unitOfWork.Lessons.GetAllAsync();
            var topics    = await _unitOfWork.Topics.GetAllAsync();
            var outcomes  = await _unitOfWork.LearningOutcomes.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();

            var result = mySubjectIds.Select(sid =>
            {
                var subject = subjects.FirstOrDefault(s => s.Id == sid);
                if (subject == null) return null;

                var subjectUnits   = units.Where(u => u.SubjectId == sid).ToList();
                var unitIds        = subjectUnits.Select(u => u.Id).ToList();
                var subjectLessons = lessons.Where(l => unitIds.Contains(l.UnitId)).ToList();
                var lessonIds      = subjectLessons.Select(l => l.Id).ToList();
                var subjectTopics  = topics.Where(t => lessonIds.Contains(t.LessonId)).ToList();
                var topicIds       = subjectTopics.Select(t => t.Id).ToList();
                var outcomeCount   = outcomes.Count(o => topicIds.Contains(o.TopicId));

                var assignedClasses = myAssignments
                    .Where(ta => ta.SubjectId == sid)
                    .Select(ta => classRooms.FirstOrDefault(c => c.Id == ta.ClassRoomId)?.Name ?? "")
                    .Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();

                return new TeacherSubjectViewModel
                {
                    SubjectId       = sid,
                    SubjectName     = subject.Name,
                    Description     = subject.Description,
                    UnitCount       = subjectUnits.Count,
                    LessonCount     = subjectLessons.Count,
                    TopicCount      = subjectTopics.Count,
                    OutcomeCount    = outcomeCount,
                    AssignedClasses = assignedClasses
                };
            }).Where(x => x != null).Cast<TeacherSubjectViewModel>().ToList();

            return View(result);
        }
    }

    public class TeacherSubjectViewModel
    {
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = default!;
        public string? Description { get; set; }
        public int UnitCount { get; set; }
        public int LessonCount { get; set; }
        public int TopicCount { get; set; }
        public int OutcomeCount { get; set; }
        public List<string> AssignedClasses { get; set; } = new();
    }
}
