using SmartEducation.Application.DTOs;

namespace SmartEducation.Application.Interfaces.Services
{
    public interface ITeacherManagementService
    {
        Task<IEnumerable<TeacherDetailDto>> GetAllAsync();
        Task<TeacherDetailDto?> GetByProfileIdAsync(Guid profileId);
        Task<(bool Success, string[] Errors)> UpdateTeacherAsync(TeacherDetailDto dto);
        Task<bool> DeactivateAsync(Guid profileId);
        Task<(bool Success, string[] Errors)> ResetPasswordAsync(Guid userId, string newPassword);
    }
}
