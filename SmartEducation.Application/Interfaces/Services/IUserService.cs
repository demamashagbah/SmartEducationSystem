using SmartEducation.Application.DTOs;

namespace SmartEducation.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<IEnumerable<UserDto>> GetByRoleAsync(string role);
        Task<UserDto?> GetByIdAsync(Guid id);
        Task<bool> ToggleActiveAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
        Task<(bool Success, string[] Errors)> CreateUserAsync(string firstName, string lastName, string email, string password, string role);
        Task<(bool Success, string[] Errors)> CreateFullUserAsync(CreateUserFullDto dto);
        Task<(bool Success, string[] Errors)> ResetPasswordAsync(Guid userId, string newPassword);
    }
}
