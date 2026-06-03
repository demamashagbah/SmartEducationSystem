using SmartEducation.Application.DTOs;

namespace SmartEducation.Application.Interfaces.Services
{
    public interface ISubjectService
    {
        Task<IEnumerable<SubjectDto>> GetAllAsync();
        Task<SubjectDto?> GetByIdAsync(Guid id);
        Task<SubjectDto> CreateAsync(SubjectDto dto);
        Task<bool> UpdateAsync(SubjectDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
