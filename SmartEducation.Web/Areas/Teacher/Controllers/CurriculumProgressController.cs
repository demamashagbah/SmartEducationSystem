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
    public class CurriculumProgressController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public CurriculumProgressController(ITeacherService teacherService, IUnitOfWork unitOfWork,
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
            var myAssignmentIds = myAssignments.Select(ta => ta.Id).ToList();

            var subjects  = await _unitOfWork.Subjects.GetAllAsync();
            var units     = await _unitOfWork.Units.GetAllAsync();
            var lessons   = await _unitOfWork.Lessons.GetAllAsync();
            var topics    = await _unitOfWork.Topics.GetAllAsync();
            var lessonPlans = await _unitOfWork.LessonPlans.GetAllAsync();
            var myLessonPlans = lessonPlans.Where(lp => myAssignmentIds.Contains(lp.TeacherAssignmentId)).ToList();

            var curriculumPlans = await _unitOfWork.CurriculumPlans.GetAllAsync();
            var curriculumPlanItems = await _unitOfWork.CurriculumPlanItems.GetAllAsync();
            var myCurriculumPlanIds = curriculumPlans
                .Where(cp => myAssignmentIds.Contains(cp.TeacherAssignmentId))
                .Select(cp => cp.Id).ToList();
            var myPlanItems = curriculumPlanItems.Where(i => myCurriculumPlanIds.Contains(i.CurriculumPlanId)).ToList();

            var progressData = new List<SubjectProgressViewModel>();

            foreach (var subjectId in mySubjectIds)
            {
                var subject = subjects.FirstOrDefault(s => s.Id == subjectId);
                if (subject == null) continue;

                var subjectUnits = units.Where(u => u.SubjectId == subjectId).ToList();
                var unitIds = subjectUnits.Select(u => u.Id).ToList();
                var subjectLessons = lessons.Where(l => unitIds.Contains(l.UnitId)).ToList();
                var lessonIds = subjectLessons.Select(l => l.Id).ToList();

                var completedLessonIds = myLessonPlans
                    .Where(lp => lp.LessonId.HasValue && lessonIds.Contains(lp.LessonId.Value))
                    .Select(lp => lp.LessonId!.Value).Distinct().ToList();

                var completedPlanItems = myPlanItems.Count(i => i.IsCompleted && i.LessonId.HasValue && lessonIds.Contains(i.LessonId.Value));

                int totalLessons = subjectLessons.Count;
                int completedLessons = Math.Max(completedLessonIds.Count, completedPlanItems);
                double coveragePct = totalLessons > 0 ? Math.Round((double)completedLessons / totalLessons * 100, 1) : 0;

                var unitProgress = subjectUnits.Select(u =>
                {
                    var unitLessons = subjectLessons.Where(l => l.UnitId == u.Id).ToList();
                    var unitLessonIds = unitLessons.Select(l => l.Id).ToList();
                    var unitTopics = topics.Where(t => unitLessonIds.Contains(t.LessonId)).ToList();
                    int done = completedLessonIds.Count(lid => unitLessonIds.Contains(lid));
                    double pct = unitLessons.Count > 0 ? Math.Round((double)done / unitLessons.Count * 100, 1) : 0;

                    return new UnitProgressViewModel
                    {
                        UnitId   = u.Id,
                        UnitName = u.Name,
                        TotalLessons = unitLessons.Count,
                        CompletedLessons = done,
                        CoveragePercent = pct,
                        TopicCount = unitTopics.Count,
                        Lessons = unitLessons.Select(l => new LessonProgressViewModel
                        {
                            LessonId   = l.Id,
                            LessonName = l.Name,
                            IsCompleted = completedLessonIds.Contains(l.Id)
                        }).ToList()
                    };
                }).ToList();

                progressData.Add(new SubjectProgressViewModel
                {
                    SubjectId       = subjectId,
                    SubjectName     = subject.Name,
                    TotalUnits      = subjectUnits.Count,
                    TotalLessons    = totalLessons,
                    CompletedLessons = completedLessons,
                    CoveragePercent = coveragePct,
                    Units = unitProgress
                });
            }

            return View(progressData);
        }
    }

    public class SubjectProgressViewModel
    {
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = default!;
        public int TotalUnits { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public double CoveragePercent { get; set; }
        public List<UnitProgressViewModel> Units { get; set; } = new();
    }

    public class UnitProgressViewModel
    {
        public Guid UnitId { get; set; }
        public string UnitName { get; set; } = default!;
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public double CoveragePercent { get; set; }
        public int TopicCount { get; set; }
        public List<LessonProgressViewModel> Lessons { get; set; } = new();
    }

    public class LessonProgressViewModel
    {
        public Guid LessonId { get; set; }
        public string LessonName { get; set; } = default!;
        public bool IsCompleted { get; set; }
    }
}
