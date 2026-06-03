using SmartEducation.Application.DTOs;

namespace SmartEducation.Application.Interfaces.Services
{
    public interface IGradeService
    {
        Task<IEnumerable<GradeDto>> GetAllAsync();
        Task<GradeDto?> GetByIdAsync(Guid id);
        Task<GradeDto> CreateAsync(GradeDto dto);
        Task<bool> UpdateAsync(GradeDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
