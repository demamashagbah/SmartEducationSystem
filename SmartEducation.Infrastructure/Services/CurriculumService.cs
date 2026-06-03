using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Infrastructure.Services
{
    public class CurriculumService : ICurriculumService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CurriculumService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =================== UNITS ===================

        public async Task<IEnumerable<UnitDto>> GetAllUnitsAsync()
        {
            var units = await _unitOfWork.Units.GetAllAsync();
            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var lessons = await _unitOfWork.Lessons.GetAllAsync();

            return units.Select(u => new UnitDto
            {
                Id = u.Id,
                Name = u.Name,
                SubjectId = u.SubjectId,
                SubjectName = subjects.FirstOrDefault(s => s.Id == u.SubjectId)?.Name ?? "",
                LessonCount = lessons.Count(l => l.UnitId == u.Id)
            });
        }

        public async Task<IEnumerable<UnitDto>> GetUnitsBySubjectAsync(Guid subjectId)
        {
            var all = await GetAllUnitsAsync();
            return all.Where(u => u.SubjectId == subjectId);
        }

        public async Task<UnitDto?> GetUnitByIdAsync(Guid id)
        {
            var u = await _unitOfWork.Units.GetByIdAsync(id);
            if (u == null) return null;
            var subject = await _unitOfWork.Subjects.GetByIdAsync(u.SubjectId);
            return new UnitDto { Id = u.Id, Name = u.Name, SubjectId = u.SubjectId, SubjectName = subject?.Name ?? "" };
        }

        public async Task<UnitDto> CreateUnitAsync(UnitDto dto)
        {
            var entity = new Domain.Entities.Unit { Id = Guid.NewGuid(), Name = dto.Name, SubjectId = dto.SubjectId };
            await _unitOfWork.Units.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> UpdateUnitAsync(UnitDto dto)
        {
            var entity = await _unitOfWork.Units.GetByIdAsync(dto.Id);
            if (entity == null) return false;
            entity.Name = dto.Name;
            entity.SubjectId = dto.SubjectId;
            await _unitOfWork.Units.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUnitAsync(Guid id)
        {
            var entity = await _unitOfWork.Units.GetByIdAsync(id);
            if (entity == null) return false;
            await _unitOfWork.Units.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // =================== LESSONS ===================

        public async Task<IEnumerable<LessonDto>> GetAllLessonsAsync()
        {
            var lessons = await _unitOfWork.Lessons.GetAllAsync();
            var units = await _unitOfWork.Units.GetAllAsync();
            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var topics = await _unitOfWork.Topics.GetAllAsync();

            return lessons.Select(l =>
            {
                var unit = units.FirstOrDefault(u => u.Id == l.UnitId);
                var subject = subjects.FirstOrDefault(s => s.Id == unit?.SubjectId);
                return new LessonDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    UnitId = l.UnitId,
                    UnitName = unit?.Name ?? "",
                    SubjectName = subject?.Name ?? "",
                    TopicCount = topics.Count(t => t.LessonId == l.Id)
                };
            });
        }

        public async Task<IEnumerable<LessonDto>> GetLessonsByUnitAsync(Guid unitId)
        {
            var all = await GetAllLessonsAsync();
            return all.Where(l => l.UnitId == unitId);
        }

        public async Task<LessonDto?> GetLessonByIdAsync(Guid id)
        {
            var l = await _unitOfWork.Lessons.GetByIdAsync(id);
            if (l == null) return null;
            var unit = await _unitOfWork.Units.GetByIdAsync(l.UnitId);
            return new LessonDto { Id = l.Id, Name = l.Name, UnitId = l.UnitId, UnitName = unit?.Name ?? "" };
        }

        public async Task<LessonDto> CreateLessonAsync(LessonDto dto)
        {
            var entity = new Lesson { Id = Guid.NewGuid(), Name = dto.Name, UnitId = dto.UnitId };
            await _unitOfWork.Lessons.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> UpdateLessonAsync(LessonDto dto)
        {
            var entity = await _unitOfWork.Lessons.GetByIdAsync(dto.Id);
            if (entity == null) return false;
            entity.Name = dto.Name;
            entity.UnitId = dto.UnitId;
            await _unitOfWork.Lessons.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteLessonAsync(Guid id)
        {
            var entity = await _unitOfWork.Lessons.GetByIdAsync(id);
            if (entity == null) return false;
            await _unitOfWork.Lessons.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // =================== TOPICS ===================

        public async Task<IEnumerable<TopicDto>> GetAllTopicsAsync()
        {
            var topics = await _unitOfWork.Topics.GetAllAsync();
            var lessons = await _unitOfWork.Lessons.GetAllAsync();
            var outcomes = await _unitOfWork.LearningOutcomes.GetAllAsync();

            return topics.Select(t => new TopicDto
            {
                Id = t.Id,
                Name = t.Name,
                LessonId = t.LessonId,
                LessonName = lessons.FirstOrDefault(l => l.Id == t.LessonId)?.Name ?? "",
                LearningOutcomeCount = outcomes.Count(lo => lo.TopicId == t.Id)
            });
        }

        public async Task<IEnumerable<TopicDto>> GetTopicsByLessonAsync(Guid lessonId)
        {
            var all = await GetAllTopicsAsync();
            return all.Where(t => t.LessonId == lessonId);
        }

        public async Task<TopicDto?> GetTopicByIdAsync(Guid id)
        {
            var t = await _unitOfWork.Topics.GetByIdAsync(id);
            if (t == null) return null;
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(t.LessonId);
            return new TopicDto { Id = t.Id, Name = t.Name, LessonId = t.LessonId, LessonName = lesson?.Name ?? "" };
        }

        public async Task<TopicDto> CreateTopicAsync(TopicDto dto)
        {
            var entity = new Topic { Id = Guid.NewGuid(), Name = dto.Name, LessonId = dto.LessonId };
            await _unitOfWork.Topics.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> UpdateTopicAsync(TopicDto dto)
        {
            var entity = await _unitOfWork.Topics.GetByIdAsync(dto.Id);
            if (entity == null) return false;
            entity.Name = dto.Name;
            entity.LessonId = dto.LessonId;
            await _unitOfWork.Topics.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTopicAsync(Guid id)
        {
            var entity = await _unitOfWork.Topics.GetByIdAsync(id);
            if (entity == null) return false;
            await _unitOfWork.Topics.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // =================== LEARNING OUTCOMES ===================

        public async Task<IEnumerable<LearningOutcomeDto>> GetAllLearningOutcomesAsync()
        {
            var outcomes = await _unitOfWork.LearningOutcomes.GetAllAsync();
            var topics = await _unitOfWork.Topics.GetAllAsync();

            return outcomes.Select(lo => new LearningOutcomeDto
            {
                Id = lo.Id,
                Description = lo.Description,
                TopicId = lo.TopicId,
                TopicName = topics.FirstOrDefault(t => t.Id == lo.TopicId)?.Name ?? ""
            });
        }

        public async Task<IEnumerable<LearningOutcomeDto>> GetLearningOutcomesByTopicAsync(Guid topicId)
        {
            var all = await GetAllLearningOutcomesAsync();
            return all.Where(lo => lo.TopicId == topicId);
        }

        public async Task<LearningOutcomeDto?> GetLearningOutcomeByIdAsync(Guid id)
        {
            var lo = await _unitOfWork.LearningOutcomes.GetByIdAsync(id);
            if (lo == null) return null;
            var topic = await _unitOfWork.Topics.GetByIdAsync(lo.TopicId);
            return new LearningOutcomeDto { Id = lo.Id, Description = lo.Description, TopicId = lo.TopicId, TopicName = topic?.Name ?? "" };
        }

        public async Task<LearningOutcomeDto> CreateLearningOutcomeAsync(LearningOutcomeDto dto)
        {
            var entity = new LearningOutcome { Id = Guid.NewGuid(), Description = dto.Description, TopicId = dto.TopicId };
            await _unitOfWork.LearningOutcomes.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> UpdateLearningOutcomeAsync(LearningOutcomeDto dto)
        {
            var entity = await _unitOfWork.LearningOutcomes.GetByIdAsync(dto.Id);
            if (entity == null) return false;
            entity.Description = dto.Description;
            entity.TopicId = dto.TopicId;
            await _unitOfWork.LearningOutcomes.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteLearningOutcomeAsync(Guid id)
        {
            var entity = await _unitOfWork.LearningOutcomes.GetByIdAsync(id);
            if (entity == null) return false;
            await _unitOfWork.LearningOutcomes.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
