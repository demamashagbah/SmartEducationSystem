using SmartEducation.Application.DTOs;

namespace SmartEducation.Application.Interfaces.Services
{
    public interface IAssignmentService
    {
        Task<IEnumerable<AssignmentDto>> GetAllAsync();
        Task<IEnumerable<AssignmentDto>> GetByTeacherAsync(Guid teacherId);
        Task<IEnumerable<AssignmentDto>> GetByClassRoomAsync(Guid classRoomId);
        Task<AssignmentDto?> GetByIdAsync(Guid id);
        Task<AssignmentDto> CreateAsync(AssignmentDto dto);
        Task<bool> UpdateAsync(AssignmentDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
