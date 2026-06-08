using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Infrastructure.Services
{
    public class TeacherGuideService : ITeacherGuideService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TeacherGuideService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TeacherGuideDto>> GetBySubjectAsync(Guid subjectId)
        {
            var guides    = await _unitOfWork.TeacherGuides.GetAllAsync();
            var subjects  = await _unitOfWork.Subjects.GetAllAsync();
            var years     = await _unitOfWork.AcademicYears.GetAllAsync();
            return guides.Where(g => g.SubjectId == subjectId)
                         .OrderByDescending(g => g.UploadedAt)
                         .Select(g => new TeacherGuideDto
                         {
                             Id             = g.Id,
                             SubjectId      = g.SubjectId,
                             SubjectName    = subjects.FirstOrDefault(s => s.Id == g.SubjectId)?.Name ?? "",
                             FileName       = g.FileName,
                             FilePath       = g.FilePath,
                             Description    = g.Description,
                             AcademicYearId = g.AcademicYearId,
                             AcademicYearName = g.AcademicYearId.HasValue
                                 ? years.FirstOrDefault(y => y.Id == g.AcademicYearId)?.Name ?? ""
                                 : "",
                             UploadedAt     = g.UploadedAt,
                             IsAnalyzed     = g.IsAnalyzed,
                             AnalysisNotes  = g.AnalysisNotes
                         });
        }

        public async Task<TeacherGuideDto> UploadGuideAsync(Guid subjectId, string fileName, string filePath,
            string? description, Guid? academicYearId = null)
        {
            var entity = new TeacherGuide
            {
                Id             = Guid.NewGuid(),
                SubjectId      = subjectId,
                FileName       = fileName,
                FilePath       = filePath,
                Description    = description,
                AcademicYearId = academicYearId,
                UploadedAt     = DateTime.UtcNow,
                IsAnalyzed     = false
            };
            await _unitOfWork.TeacherGuides.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var subject = await _unitOfWork.Subjects.GetByIdAsync(subjectId);
            var years   = await _unitOfWork.AcademicYears.GetAllAsync();
            return new TeacherGuideDto
            {
                Id               = entity.Id,
                SubjectId        = subjectId,
                SubjectName      = subject?.Name ?? "",
                FileName         = fileName,
                FilePath         = filePath,
                Description      = description,
                AcademicYearId   = academicYearId,
                AcademicYearName = academicYearId.HasValue
                    ? years.FirstOrDefault(y => y.Id == academicYearId)?.Name ?? ""
                    : "",
                UploadedAt  = entity.UploadedAt,
                IsAnalyzed  = false
            };
        }

        public async Task<bool> DeleteGuideAsync(Guid id)
        {
            var entity = await _unitOfWork.TeacherGuides.GetByIdAsync(id);
            if (entity == null) return false;
            await _unitOfWork.TeacherGuides.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<SubjectCurriculumDto> GetSubjectCurriculumAsync(Guid subjectId)
        {
            var subject = await _unitOfWork.Subjects.GetByIdAsync(subjectId);
            if (subject == null) return new SubjectCurriculumDto();

            var classRooms   = await _unitOfWork.ClassRooms.GetAllAsync();
            var classRoom    = classRooms.FirstOrDefault(c => c.Id == subject.ClassRoomId);
            var academicYears = await _unitOfWork.AcademicYears.GetAllAsync();

            var allUnits    = await _unitOfWork.Units.GetAllAsync();
            var allLessons  = await _unitOfWork.Lessons.GetAllAsync();
            var allTopics   = await _unitOfWork.Topics.GetAllAsync();
            var allOutcomes = await _unitOfWork.LearningOutcomes.GetAllAsync();
            var guides      = (await GetBySubjectAsync(subjectId)).ToList();

            var units    = allUnits.Where(u => u.SubjectId == subjectId).ToList();
            var unitDtos = new List<UnitCurriculumDto>();
            int totalLessons = 0, totalTopics = 0, totalOutcomes = 0, totalActivities = 0;

            foreach (var unit in units)
            {
                var lessons    = allLessons.Where(l => l.UnitId == unit.Id).ToList();
                totalLessons  += lessons.Count;
                var lessonDtos = new List<LessonCurriculumDto>();

                foreach (var lesson in lessons)
                {
                    var topics    = allTopics.Where(t => t.LessonId == lesson.Id).ToList();
                    totalTopics  += topics.Count;
                    var topicDtos = new List<TopicCurriculumDto>();

                    foreach (var topic in topics)
                    {
                        var outcomes   = allOutcomes.Where(lo => lo.TopicId == topic.Id).ToList();
                        totalOutcomes += outcomes.Count;
                        if (!string.IsNullOrEmpty(topic.Activities)) totalActivities++;

                        topicDtos.Add(new TopicCurriculumDto
                        {
                            Id                   = topic.Id,
                            Name                 = topic.Name,
                            LearningOutcomes     = outcomes.Select(lo => lo.Description).ToList(),
                            Activities           = topic.Activities,
                            TeacherNotes         = topic.TeacherNotes,
                            TeachingStrategies   = topic.TeachingStrategies,
                            AssessmentSuggestions = topic.AssessmentSuggestions,
                            HomeworkSuggestions  = topic.HomeworkSuggestions
                        });
                    }

                    lessonDtos.Add(new LessonCurriculumDto { Id = lesson.Id, Name = lesson.Name, Topics = topicDtos });
                }

                unitDtos.Add(new UnitCurriculumDto { Id = unit.Id, Name = unit.Name, Lessons = lessonDtos });
            }

            // Academic year from latest guide
            var latestGuide = guides.OrderByDescending(g => g.UploadedAt).FirstOrDefault();
            var academicYearName = latestGuide?.AcademicYearName ?? "";
            var academicYearId   = latestGuide?.AcademicYearId;

            return new SubjectCurriculumDto
            {
                SubjectId            = subjectId,
                SubjectName          = subject.Name,
                Description          = subject.Description,
                ClassRoomId          = subject.ClassRoomId ?? Guid.Empty,
                ClassRoomName        = classRoom?.Name ?? "",
                AcademicYearId       = academicYearId,
                AcademicYearName     = academicYearName,
                Units                = unitDtos,
                TeacherGuides        = guides,
                TotalUnits           = units.Count,
                TotalLessons         = totalLessons,
                TotalTopics          = totalTopics,
                TotalLearningOutcomes = totalOutcomes,
                TotalActivities      = totalActivities
            };
        }
    }
}
