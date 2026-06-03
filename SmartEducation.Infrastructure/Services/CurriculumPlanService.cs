using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Infrastructure.Services
{
    public class CurriculumPlanService : ICurriculumPlanService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CurriculumPlanService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CurriculumPlanDto>> GetByTeacherAsync(Guid teacherProfileId)
        {
            var allPlans = await _unitOfWork.CurriculumPlans.GetAllAsync();
            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var myAssignmentIds = teacherAssignments.Where(ta => ta.TeacherId == teacherProfileId)
                                                     .Select(ta => ta.Id).ToList();
            var myPlans = allPlans.Where(p => myAssignmentIds.Contains(p.TeacherAssignmentId)).ToList();

            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var allItems = await _unitOfWork.CurriculumPlanItems.GetAllAsync();

            return myPlans.Select(p =>
            {
                var ta = teacherAssignments.FirstOrDefault(t => t.Id == p.TeacherAssignmentId);
                var subject = ta != null ? subjects.FirstOrDefault(s => s.Id == ta.SubjectId) : null;
                var classRoom = ta != null ? classRooms.FirstOrDefault(c => c.Id == ta.ClassRoomId) : null;
                var items = allItems.Where(i => i.CurriculumPlanId == p.Id).ToList();

                return new CurriculumPlanDto
                {
                    Id = p.Id,
                    TeacherAssignmentId = p.TeacherAssignmentId,
                    SubjectName = subject?.Name ?? "",
                    ClassRoomName = classRoom?.Name ?? "",
                    Title = p.Title,
                    PlanType = p.PlanType,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Notes = p.Notes,
                    IsActive = p.IsActive,
                    IsAiGenerated = p.IsAiGenerated,
                    TotalItems = items.Count,
                    CompletedItems = items.Count(i => i.IsCompleted)
                };
            }).OrderByDescending(p => p.StartDate).ToList();
        }

        public async Task<CurriculumPlanDto?> GetByIdAsync(Guid id)
        {
            var plan = await _unitOfWork.CurriculumPlans.GetByIdAsync(id);
            if (plan == null) return null;

            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var ta = teacherAssignments.FirstOrDefault(t => t.Id == plan.TeacherAssignmentId);
            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var subject = ta != null ? subjects.FirstOrDefault(s => s.Id == ta.SubjectId) : null;
            var classRoom = ta != null ? classRooms.FirstOrDefault(c => c.Id == ta.ClassRoomId) : null;

            var allItems = await _unitOfWork.CurriculumPlanItems.GetAllAsync();
            var items = allItems.Where(i => i.CurriculumPlanId == id).OrderBy(i => i.PlannedDate).ToList();
            var lessons = await _unitOfWork.Lessons.GetAllAsync();
            var topics = await _unitOfWork.Topics.GetAllAsync();

            var itemDtos = items.Select(i => new CurriculumPlanItemDto
            {
                Id = i.Id,
                CurriculumPlanId = i.CurriculumPlanId,
                LessonId = i.LessonId,
                LessonName = i.LessonId.HasValue ? lessons.FirstOrDefault(l => l.Id == i.LessonId)?.Name : null,
                TopicId = i.TopicId,
                TopicName = i.TopicId.HasValue ? topics.FirstOrDefault(t => t.Id == i.TopicId)?.Name : null,
                WeekNumber = i.WeekNumber,
                PlannedDate = i.PlannedDate,
                LearningOutcomes = i.LearningOutcomes,
                TeachingStrategy = i.TeachingStrategy,
                Activities = i.Activities,
                Homework = i.Homework,
                QuizDate = i.QuizDate,
                ExamDate = i.ExamDate,
                IsCompleted = i.IsCompleted,
                CompletedDate = i.CompletedDate,
                Notes = i.Notes
            }).ToList();

            return new CurriculumPlanDto
            {
                Id = plan.Id,
                TeacherAssignmentId = plan.TeacherAssignmentId,
                SubjectName = subject?.Name ?? "",
                ClassRoomName = classRoom?.Name ?? "",
                Title = plan.Title,
                PlanType = plan.PlanType,
                StartDate = plan.StartDate,
                EndDate = plan.EndDate,
                Notes = plan.Notes,
                IsActive = plan.IsActive,
                IsAiGenerated = plan.IsAiGenerated,
                Items = itemDtos,
                TotalItems = itemDtos.Count,
                CompletedItems = itemDtos.Count(i => i.IsCompleted)
            };
        }

        public async Task<CurriculumPlanDto> GeneratePlanAsync(GenerateCurriculumPlanRequest request)
        {
            var teacherAssignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var ta = teacherAssignments.FirstOrDefault(t => t.Id == request.TeacherAssignmentId);
            if (ta == null) throw new InvalidOperationException("Teacher assignment not found.");

            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var subject = subjects.FirstOrDefault(s => s.Id == ta.SubjectId);
            var classRoom = classRooms.FirstOrDefault(c => c.Id == ta.ClassRoomId);

            // Load curriculum hierarchy
            var allUnits = await _unitOfWork.Units.GetAllAsync();
            var allLessons = await _unitOfWork.Lessons.GetAllAsync();
            var allTopics = await _unitOfWork.Topics.GetAllAsync();
            var allOutcomes = await _unitOfWork.LearningOutcomes.GetAllAsync();

            var subjectUnits = allUnits.Where(u => u.SubjectId == ta.SubjectId).ToList();
            var subjectLessons = allLessons.Where(l => subjectUnits.Any(u => u.Id == l.UnitId)).ToList();

            // Calculate end date based on plan type
            var endDate = request.PlanType switch
            {
                "Monthly" => request.StartDate.AddMonths(1),
                "Semester" => request.StartDate.AddMonths(4),
                _ => request.StartDate.AddDays(7 * Math.Max(1, (int)Math.Ceiling((double)subjectLessons.Count / request.LessonsPerWeek)))
            };

            // Teaching strategies pool
            var strategies = new[] {
                "Direct Instruction", "Cooperative Learning", "Inquiry-Based Learning",
                "Project-Based Learning", "Flipped Classroom", "Discussion-Based Learning"
            };

            // Generate plan
            var plan = new CurriculumPlan
            {
                Id = Guid.NewGuid(),
                TeacherAssignmentId = request.TeacherAssignmentId,
                Title = $"{subject?.Name ?? "Curriculum"} Plan — {request.PlanType} ({request.StartDate:MMM yyyy})",
                PlanType = request.PlanType,
                StartDate = request.StartDate,
                EndDate = endDate,
                Notes = request.Notes,
                IsActive = true,
                IsAiGenerated = true
            };

            await _unitOfWork.CurriculumPlans.AddAsync(plan);
            await _unitOfWork.SaveChangesAsync();

            // Generate items
            var currentDate = request.StartDate;
            var weekNumber = 1;
            var lessonIndex = 0;
            var strategyIndex = 0;

            foreach (var lesson in subjectLessons)
            {
                var lessonTopics = allTopics.Where(t => t.LessonId == lesson.Id).ToList();
                var topic = lessonTopics.FirstOrDefault();
                var topicOutcomes = topic != null
                    ? allOutcomes.Where(lo => lo.TopicId == topic.Id).Select(lo => lo.Description).ToList()
                    : new List<string>();

                var strategy = strategies[strategyIndex % strategies.Length];
                var activities = BuildActivities(lesson.Name, topic?.Name, strategy);
                var homework = BuildHomework(lesson.Name, topic?.Name);

                // Skip weekends
                while (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                    currentDate = currentDate.AddDays(1);

                var item = new CurriculumPlanItem
                {
                    Id = Guid.NewGuid(),
                    CurriculumPlanId = plan.Id,
                    LessonId = lesson.Id,
                    TopicId = topic?.Id,
                    WeekNumber = weekNumber,
                    PlannedDate = currentDate,
                    LearningOutcomes = topicOutcomes.Any()
                        ? string.Join("; ", topicOutcomes.Take(3))
                        : $"Students will understand the core concepts of {lesson.Name}.",
                    TeachingStrategy = strategy,
                    Activities = activities,
                    Homework = homework,
                    IsCompleted = false
                };

                await _unitOfWork.CurriculumPlanItems.AddAsync(item);
                lessonIndex++;
                strategyIndex++;
                currentDate = currentDate.AddDays(1);

                // Skip weekends after incrementing
                while (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                    currentDate = currentDate.AddDays(1);

                if (lessonIndex % request.LessonsPerWeek == 0)
                {
                    weekNumber++;
                    // Jump to next week's start if needed
                    while (currentDate.DayOfWeek != DayOfWeek.Sunday && currentDate.DayOfWeek != DayOfWeek.Monday)
                        currentDate = currentDate.AddDays(1);
                }
            }

            // Add quiz/exam markers at the end of each unit
            var unitItemDates = new Dictionary<Guid, DateTime>();
            var dateTracker = request.StartDate;
            var lessonCount = 0;
            foreach (var unit in subjectUnits)
            {
                var unitLessons = subjectLessons.Where(l => subjectUnits.Any(u => u.Id == unit.Id && l.UnitId == u.Id)).ToList();
                lessonCount += unitLessons.Count;
                var quizDate = request.StartDate.AddDays(lessonCount * 2);
                while (quizDate.DayOfWeek == DayOfWeek.Saturday || quizDate.DayOfWeek == DayOfWeek.Sunday)
                    quizDate = quizDate.AddDays(1);
                unitItemDates[unit.Id] = quizDate;
            }

            await _unitOfWork.SaveChangesAsync();

            return (await GetByIdAsync(plan.Id))!;
        }

        private static string BuildActivities(string lessonName, string? topicName, string strategy)
        {
            return strategy switch
            {
                "Cooperative Learning" => $"1. Divide students into groups of 4.\n2. Each group researches {topicName ?? lessonName} and prepares a short presentation.\n3. Groups present findings and class discusses key points.\n4. Individual reflection journal entry.",
                "Inquiry-Based Learning" => $"1. Pose a guiding question about {topicName ?? lessonName}.\n2. Students form hypotheses.\n3. Investigation and data collection activity.\n4. Share findings and draw conclusions.",
                "Project-Based Learning" => $"1. Introduce the project: Create a model/poster related to {topicName ?? lessonName}.\n2. Students plan and design their project.\n3. Work session with teacher guidance.\n4. Present to class.",
                "Flipped Classroom" => $"1. Review video assigned as homework on {topicName ?? lessonName}.\n2. Q&A to clarify concepts.\n3. Hands-on practice exercises in pairs.\n4. Class discussion and summary.",
                "Discussion-Based Learning" => $"1. Open discussion: What do you already know about {topicName ?? lessonName}?\n2. Structured debate or think-pair-share activity.\n3. Teacher-facilitated synthesis of ideas.\n4. Exit ticket: 3 things learned.",
                _ => $"1. Introduction and objectives review for {lessonName}.\n2. Teacher explanation with examples.\n3. Guided practice exercises.\n4. Independent practice and Q&A.\n5. Lesson summary and key takeaways."
            };
        }

        private static string BuildHomework(string lessonName, string? topicName)
        {
            var tasks = new[]
            {
                $"Complete exercises 1–5 on {topicName ?? lessonName} from the textbook.",
                $"Write a one-page reflection on what you learned about {topicName ?? lessonName}.",
                $"Research and find a real-world example related to {topicName ?? lessonName}.",
                $"Create a mind map summarizing the key concepts of {topicName ?? lessonName}.",
                $"Practice the problems on pages related to {lessonName} and check your answers."
            };
            return tasks[Math.Abs((lessonName + (topicName ?? "")).GetHashCode()) % tasks.Length];
        }

        public async Task<bool> MarkItemCompletedAsync(Guid itemId, bool completed)
        {
            var item = await _unitOfWork.CurriculumPlanItems.GetByIdAsync(itemId);
            if (item == null) return false;
            item.IsCompleted = completed;
            item.CompletedDate = completed ? DateTime.UtcNow : null;
            await _unitOfWork.CurriculumPlanItems.UpdateAsync(item);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateItemAsync(CurriculumPlanItemDto dto)
        {
            var item = await _unitOfWork.CurriculumPlanItems.GetByIdAsync(dto.Id);
            if (item == null) return false;
            item.TeachingStrategy = dto.TeachingStrategy;
            item.Activities = dto.Activities;
            item.Homework = dto.Homework;
            item.LearningOutcomes = dto.LearningOutcomes;
            item.QuizDate = dto.QuizDate;
            item.ExamDate = dto.ExamDate;
            item.Notes = dto.Notes;
            item.PlannedDate = dto.PlannedDate;
            await _unitOfWork.CurriculumPlanItems.UpdateAsync(item);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePlanAsync(Guid id)
        {
            var plan = await _unitOfWork.CurriculumPlans.GetByIdAsync(id);
            if (plan == null) return false;
            await _unitOfWork.CurriculumPlans.DeleteAsync(plan);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<CurriculumPlanDto> GetProgressAsync(Guid planId)
        {
            return (await GetByIdAsync(planId)) ?? new CurriculumPlanDto();
        }
    }
}
