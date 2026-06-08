using Microsoft.AspNetCore.Identity;
using SmartEducation.Application.Constants;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = _userManager.Users.ToList();
            var result = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(MapToDto(user, roles.FirstOrDefault() ?? "N/A"));
            }
            return result;
        }

        public async Task<IEnumerable<UserDto>> GetByRoleAsync(string role)
        {
            var all = await GetAllAsync();
            return all.Where(u => u.Role == role);
        }

        public async Task<UserDto?> GetByIdAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return null;
            var roles = await _userManager.GetRolesAsync(user);
            return MapToDto(user, roles.FirstOrDefault() ?? "N/A");
        }

        public async Task<bool> ToggleActiveAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return false;
            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return false;
            user.IsActive = false;
            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<(bool Success, string[] Errors)> CreateUserAsync(
            string firstName, string lastName, string email, string password, string role)
        {
            return await CreateFullUserAsync(new CreateUserFullDto
            {
                FirstName = firstName,
                LastName = lastName,
                Username = email,
                Email = email,
                Password = password,
                Role = role
            });
        }

        public async Task<(bool Success, string[] Errors)> CreateFullUserAsync(CreateUserFullDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = string.IsNullOrWhiteSpace(dto.Username) ? dto.Email : dto.Username,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                Position = dto.Position,
                Department = dto.Department,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return (false, result.Errors.Select(e => e.Description).ToArray());

            await _userManager.AddToRoleAsync(user, dto.Role);

            if (dto.Role == Roles.Teacher)
            {
                var employeeNumber = $"EMP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
                await _unitOfWork.TeacherProfiles.AddAsync(new TeacherProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    EmployeeNumber = employeeNumber,
                    Specialization = dto.Specialization,
                    Qualification = dto.Qualification,
                    YearsOfExperience = dto.YearsOfExperience
                });
                await _unitOfWork.SaveChangesAsync();
            }
            else if (dto.Role == Roles.Student)
            {
                await _unitOfWork.StudentProfiles.AddAsync(new StudentProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    ClassRoomId = null, // Assigned to class separately
                    StudentNumber = dto.StudentNumber ?? $"STU-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
                    NationalNumber = dto.NationalNumber ?? "",
                    AcademicYearId = dto.AcademicYearId,
                    EnrollmentDate = dto.EnrollmentDate ?? DateTime.UtcNow,
                    ParentName = dto.ParentName ?? "",
                    ParentPhone = dto.ParentPhone ?? "",
                    ParentEmail = dto.ParentEmail ?? "",
                    Address = dto.Address,
                    EmergencyContact = dto.EmergencyContact
                });
                await _unitOfWork.SaveChangesAsync();
            }
            else if (dto.Role == Roles.Parent)
            {
                await _unitOfWork.ParentProfiles.AddAsync(new ParentProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Occupation = dto.Occupation,
                    EmergencyContact = dto.EmergencyContact
                });
                await _unitOfWork.SaveChangesAsync();
            }

            return (true, Array.Empty<string>());
        }

        public async Task<(bool Success, string[] Errors)> ResetPasswordAsync(Guid userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return (false, new[] { "User not found." });
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result.Succeeded
                ? (true, Array.Empty<string>())
                : (false, result.Errors.Select(e => e.Description).ToArray());
        }

        private static UserDto MapToDto(ApplicationUser user, string role) => new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.UserName ?? "",
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            IsActive = user.IsActive,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
    }
}
