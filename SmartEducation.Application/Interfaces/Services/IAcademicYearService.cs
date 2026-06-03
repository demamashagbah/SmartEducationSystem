using SmartEducation.Application.DTOs;

namespace SmartEducation.Application.Interfaces.Services
{
    public interface IAcademicYearService
    {
        Task<IEnumerable<AcademicYearDto>> GetAllAsync();
        Task<AcademicYearDto?> GetByIdAsync(Guid id);
        Task<AcademicYearDto> CreateAsync(AcademicYearDto dto);
        Task<bool> UpdateAsync(AcademicYearDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
