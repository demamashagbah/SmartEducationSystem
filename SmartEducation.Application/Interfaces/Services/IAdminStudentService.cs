using SmartEducation.Application.DTOs;

namespace SmartEducation.Application.Interfaces.Services
{
    public interface IAdminStudentService
    {
        // Queries
        Task<IEnumerable<StudentDetailDto>> GetAllAsync();
        Task<IEnumerable<StudentDetailDto>> GetByClassRoomAsync(Guid classRoomId);
        Task<IEnumerable<StudentDetailDto>> GetAvailableForClassAsync(Guid classRoomId);
        Task<StudentDetailDto?> GetByProfileIdAsync(Guid profileId);

        // Class assignment
        Task<bool> AssignToClassAsync(Guid profileId, Guid classRoomId);
        Task<bool> RemoveFromClassAsync(Guid profileId);
        Task<bool> TransferStudentAsync(Guid profileId, Guid newClassRoomId);

        // Profile updates (edit from class management)
        Task<(bool Success, string[] Errors)> UpdateStudentAsync(StudentDetailDto dto);

        // Password
        Task<(bool Success, string[] Errors)> ResetPasswordAsync(Guid userId, string newPassword);
    }
}
