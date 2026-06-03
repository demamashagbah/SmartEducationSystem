using SmartEducation.Application.DTOs;

namespace SmartEducation.Application.Interfaces.Services
{
    public interface ICurriculumService
    {
        // Units
        Task<IEnumerable<UnitDto>> GetAllUnitsAsync();
        Task<IEnumerable<UnitDto>> GetUnitsBySubjectAsync(Guid subjectId);
        Task<UnitDto?> GetUnitByIdAsync(Guid id);
        Task<UnitDto> CreateUnitAsync(UnitDto dto);
        Task<bool> UpdateUnitAsync(UnitDto dto);
        Task<bool> DeleteUnitAsync(Guid id);

        // Lessons
        Task<IEnumerable<LessonDto>> GetAllLessonsAsync();
        Task<IEnumerable<LessonDto>> GetLessonsByUnitAsync(Guid unitId);
        Task<LessonDto?> GetLessonByIdAsync(Guid id);
        Task<LessonDto> CreateLessonAsync(LessonDto dto);
        Task<bool> UpdateLessonAsync(LessonDto dto);
        Task<bool> DeleteLessonAsync(Guid id);

        // Topics
        Task<IEnumerable<TopicDto>> GetAllTopicsAsync();
        Task<IEnumerable<TopicDto>> GetTopicsByLessonAsync(Guid lessonId);
        Task<TopicDto?> GetTopicByIdAsync(Guid id);
        Task<TopicDto> CreateTopicAsync(TopicDto dto);
        Task<bool> UpdateTopicAsync(TopicDto dto);
        Task<bool> DeleteTopicAsync(Guid id);

        // Learning Outcomes
        Task<IEnumerable<LearningOutcomeDto>> GetAllLearningOutcomesAsync();
        Task<IEnumerable<LearningOutcomeDto>> GetLearningOutcomesByTopicAsync(Guid topicId);
        Task<LearningOutcomeDto?> GetLearningOutcomeByIdAsync(Guid id);
        Task<LearningOutcomeDto> CreateLearningOutcomeAsync(LearningOutcomeDto dto);
        Task<bool> UpdateLearningOutcomeAsync(LearningOutcomeDto dto);
        Task<bool> DeleteLearningOutcomeAsync(Guid id);
    }
}
