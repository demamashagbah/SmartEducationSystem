using SmartEducation.Application.DTOs;

namespace SmartEducation.Application.Interfaces.Services
{
    public interface IClassRoomService
    {
        Task<IEnumerable<ClassRoomDto>> GetAllAsync();
        Task<ClassRoomDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<ClassRoomDto>> GetByGradeAsync(Guid gradeId);
        Task<ClassRoomDto> CreateAsync(ClassRoomDto dto);
        Task<bool> UpdateAsync(ClassRoomDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
