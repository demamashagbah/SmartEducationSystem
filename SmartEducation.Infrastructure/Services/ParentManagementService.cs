using Microsoft.AspNetCore.Identity;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Infrastructure.Services
{
    public class ParentManagementService : IParentManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public ParentManagementService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IEnumerable<ParentDetailDto>> GetAllAsync()
        {
            var profiles = await _unitOfWork.ParentProfiles.GetAllAsync();
            var result = new List<ParentDetailDto>();
            foreach (var profile in profiles)
            {
                var user = await _userManager.FindByIdAsync(profile.UserId.ToString());
                if (user == null) continue;
                result.Add(await BuildDto(profile, user));
            }
            return result;
        }

        public async Task<ParentDetailDto?> GetByProfileIdAsync(Guid profileId)
        {
            var profile = await _unitOfWork.ParentProfiles.GetByIdAsync(profileId);
            if (profile == null) return null;
            var user = await _userManager.FindByIdAsync(profile.UserId.ToString());
            if (user == null) return null;
            return await BuildDto(profile, user);
        }

        public async Task<(bool Success, string[] Errors)> UpdateParentAsync(ParentDetailDto dto)
        {
            var profile = await _unitOfWork.ParentProfiles.GetByIdAsync(dto.ProfileId);
            if (profile == null) return (false, new[] { "Parent profile not found." });

            var user = await _userManager.FindByIdAsync(profile.UserId.ToString());
            if (user == null) return (false, new[] { "User account not found." });

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.UserName = dto.Username;
            user.PhoneNumber = dto.PhoneNumber;
            user.Gender = dto.Gender;
            user.DateOfBirth = dto.DateOfBirth;
            var userResult = await _userManager.UpdateAsync(user);
            if (!userResult.Succeeded)
                return (false, userResult.Errors.Select(e => e.Description).ToArray());

            profile.Occupation = dto.Occupation;
            profile.EmergencyContact = dto.EmergencyContact;
            await _unitOfWork.ParentProfiles.UpdateAsync(profile);
            await _unitOfWork.SaveChangesAsync();
            return (true, Array.Empty<string>());
        }

        public async Task<bool> DeactivateAsync(Guid profileId)
        {
            var profile = await _unitOfWork.ParentProfiles.GetByIdAsync(profileId);
            if (profile == null) return false;
            var user = await _userManager.FindByIdAsync(profile.UserId.ToString());
            if (user == null) return false;
            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
            return true;
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

        private async Task<ParentDetailDto> BuildDto(ParentProfile profile, ApplicationUser user)
        {
            var parentStudents = profile.ParentStudents ?? new List<ParentStudent>();
            var studentProfiles = await _unitOfWork.StudentProfiles.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var academicYears = await _unitOfWork.AcademicYears.GetAllAsync();

            var children = new List<StudentDetailDto>();
            foreach (var ps in parentStudents)
            {
                var sp = studentProfiles.FirstOrDefault(s => s.Id == ps.StudentId);
                if (sp == null) continue;
                var sUser = await _userManager.FindByIdAsync(sp.UserId.ToString());
                if (sUser == null) continue;
                children.Add(new StudentDetailDto
                {
                    ProfileId = sp.Id,
                    UserId = sUser.Id,
                    FirstName = sUser.FirstName,
                    LastName = sUser.LastName,
                    Email = sUser.Email ?? "",
                    StudentNumber = sp.StudentNumber,
                    ClassRoomId = sp.ClassRoomId,
                    ClassRoomName = sp.ClassRoomId.HasValue
                        ? classRooms.FirstOrDefault(c => c.Id == sp.ClassRoomId)?.Name ?? ""
                        : "Unassigned",
                    IsActive = sUser.IsActive
                });
            }

            return new ParentDetailDto
            {
                ProfileId = profile.Id,
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                Occupation = profile.Occupation,
                EmergencyContact = profile.EmergencyContact,
                IsActive = user.IsActive,
                Children = children
            };
        }
    }
}
