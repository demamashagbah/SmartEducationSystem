using SmartEducation.Application.DTOs;

namespace SmartEducation.Application.Interfaces.Services
{
    public interface ITeacherGuideService
    {
        Task<IEnumerable<TeacherGuideDto>> GetBySubjectAsync(Guid subjectId);
        Task<TeacherGuideDto> UploadGuideAsync(Guid subjectId, string fileName, string filePath, string? description, Guid? academicYearId = null);
        Task<bool> DeleteGuideAsync(Guid id);
        Task<SubjectCurriculumDto> GetSubjectCurriculumAsync(Guid subjectId);
    }
}
