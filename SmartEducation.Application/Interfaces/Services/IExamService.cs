using SmartEducation.Application.DTOs;

namespace SmartEducation.Application.Interfaces.Services
{
    public interface IExamService
    {
        Task<IEnumerable<ExamDto>> GetAllAsync();
        Task<IEnumerable<ExamDto>> GetBySubjectAsync(Guid subjectId);
        Task<ExamDto?> GetByIdAsync(Guid id);
        Task<ExamDto> CreateAsync(ExamDto dto);
        Task<bool> UpdateAsync(ExamDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
