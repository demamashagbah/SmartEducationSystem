using Microsoft.AspNetCore.Identity;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Infrastructure.Services
{
    public class TeacherManagementService : ITeacherManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeacherManagementService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IEnumerable<TeacherDetailDto>> GetAllAsync()
        {
            var profiles = await _unitOfWork.TeacherProfiles.GetAllAsync();
            var assignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();

            var result = new List<TeacherDetailDto>();
            foreach (var profile in profiles)
            {
                var user = await _userManager.FindByIdAsync(profile.UserId.ToString());
                if (user == null) continue;

                var myAssignments = assignments.Where(a => a.TeacherId == profile.Id && a.IsActive).ToList();
                result.Add(new TeacherDetailDto
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
                    EmployeeNumber = profile.EmployeeNumber,
                    Specialization = profile.Specialization,
                    Qualification = profile.Qualification,
                    YearsOfExperience = profile.YearsOfExperience,
                    IsActive = user.IsActive,
                    AssignedClassCount = myAssignments.Select(a => a.ClassRoomId).Distinct().Count(),
                    AssignedSubjectCount = myAssignments.Select(a => a.SubjectId).Distinct().Count(),
                    AssignedClassNames = myAssignments
                        .Select(a => classRooms.FirstOrDefault(c => c.Id == a.ClassRoomId)?.Name ?? "")
                        .Where(n => n != "").Distinct().ToList(),
                    AssignedSubjectNames = myAssignments
                        .Select(a => subjects.FirstOrDefault(s => s.Id == a.SubjectId)?.Name ?? "")
                        .Where(n => n != "").Distinct().ToList()
                });
            }
            return result;
        }

        public async Task<TeacherDetailDto?> GetByProfileIdAsync(Guid profileId)
        {
            var all = await GetAllAsync();
            return all.FirstOrDefault(t => t.ProfileId == profileId);
        }

        public async Task<(bool Success, string[] Errors)> UpdateTeacherAsync(TeacherDetailDto dto)
        {
            var profile = await _unitOfWork.TeacherProfiles.GetByIdAsync(dto.ProfileId);
            if (profile == null) return (false, new[] { "Teacher profile not found." });

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

            profile.Specialization = dto.Specialization;
            profile.Qualification = dto.Qualification;
            profile.YearsOfExperience = dto.YearsOfExperience;
            await _unitOfWork.TeacherProfiles.UpdateAsync(profile);
            await _unitOfWork.SaveChangesAsync();
            return (true, Array.Empty<string>());
        }

        public async Task<bool> DeactivateAsync(Guid profileId)
        {
            var profile = await _unitOfWork.TeacherProfiles.GetByIdAsync(profileId);
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
    }
}
