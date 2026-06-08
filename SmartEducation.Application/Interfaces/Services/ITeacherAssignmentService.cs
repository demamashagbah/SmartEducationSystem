using SmartEducation.Application.DTOs;

namespace SmartEducation.Application.Interfaces.Services
{
    public interface ITeacherAssignmentService
    {
        Task<IEnumerable<TeacherAssignmentDetailDto>> GetAllAsync();
        Task<IEnumerable<TeacherAssignmentDetailDto>> GetByClassRoomAsync(Guid classRoomId);
        Task<TeacherAssignmentDetailDto?> GetByIdAsync(Guid id);
        Task<(bool Success, string Error)> AssignTeacherAsync(TeacherAssignmentDetailDto dto);
        Task<bool> RemoveAssignmentAsync(Guid id);
        Task<bool> ToggleActiveAsync(Guid id);
    }
}
