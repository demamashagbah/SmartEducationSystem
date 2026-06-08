using SmartEducation.Application.DTOs;

namespace SmartEducation.Application.Interfaces.Services
{
    public interface IParentManagementService
    {
        Task<IEnumerable<ParentDetailDto>> GetAllAsync();
        Task<ParentDetailDto?> GetByProfileIdAsync(Guid profileId);
        Task<(bool Success, string[] Errors)> UpdateParentAsync(ParentDetailDto dto);
        Task<bool> DeactivateAsync(Guid profileId);
        Task<(bool Success, string[] Errors)> ResetPasswordAsync(Guid userId, string newPassword);
    }
}
