using Microsoft.AspNetCore.Identity;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Application.Constants;
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
            var users = _userManager.Users.Where(u => u.IsActive || !u.IsActive).ToList();
            var result = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    Role = roles.FirstOrDefault() ?? "N/A",
                    CreatedAt = DateTime.UtcNow
                });
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
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                Role = roles.FirstOrDefault() ?? "N/A"
            };
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

        public async Task<(bool Success, string[] Errors)> CreateUserAsync(string firstName, string lastName, string email, string password, string role)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return (false, result.Errors.Select(e => e.Description).ToArray());

            await _userManager.AddToRoleAsync(user, role);

            if (role == Roles.Teacher)
            {
                var employeeNumber = $"EMP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
                await _unitOfWork.TeacherProfiles.AddAsync(new TeacherProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    EmployeeNumber = employeeNumber
                });
                await _unitOfWork.SaveChangesAsync();
            }
            else if (role == Roles.Student)
            {
                var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
                var firstRoom = classRooms.FirstOrDefault();
                if (firstRoom != null)
                {
                    await _unitOfWork.StudentProfiles.AddAsync(new StudentProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        ClassRoomId = firstRoom.Id
                    });
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            else if (role == Roles.Parent)
            {
                await _unitOfWork.ParentProfiles.AddAsync(new ParentProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id
                });
                await _unitOfWork.SaveChangesAsync();
            }

            return (true, Array.Empty<string>());
        }
    }
}
