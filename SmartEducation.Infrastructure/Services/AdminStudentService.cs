using Microsoft.AspNetCore.Identity;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Infrastructure.Services
{
    public class AdminStudentService : IAdminStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminStudentService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IEnumerable<StudentDetailDto>> GetAllAsync()
        {
            var profiles = await _unitOfWork.StudentProfiles.GetAllAsync();
            return await BuildDtos(profiles);
        }

        public async Task<IEnumerable<StudentDetailDto>> GetByClassRoomAsync(Guid classRoomId)
        {
            var profiles = await _unitOfWork.StudentProfiles.GetAllAsync();
            return await BuildDtos(profiles.Where(p => p.ClassRoomId == classRoomId));
        }

        public async Task<IEnumerable<StudentDetailDto>> GetAvailableForClassAsync(Guid classRoomId)
        {
            var profiles = await _unitOfWork.StudentProfiles.GetAllAsync();
            return await BuildDtos(profiles.Where(p => p.ClassRoomId != classRoomId));
        }

        public async Task<StudentDetailDto?> GetByProfileIdAsync(Guid profileId)
        {
            var profile = await _unitOfWork.StudentProfiles.GetByIdAsync(profileId);
            if (profile == null) return null;
            var dtos = await BuildDtos(new[] { profile });
            return dtos.FirstOrDefault();
        }

        public async Task<bool> AssignToClassAsync(Guid profileId, Guid classRoomId)
        {
            var profile = await _unitOfWork.StudentProfiles.GetByIdAsync(profileId);
            if (profile == null) return false;
            profile.ClassRoomId = classRoomId;
            await _unitOfWork.StudentProfiles.UpdateAsync(profile);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromClassAsync(Guid profileId)
        {
            var profile = await _unitOfWork.StudentProfiles.GetByIdAsync(profileId);
            if (profile == null) return false;
            profile.ClassRoomId = null;
            await _unitOfWork.StudentProfiles.UpdateAsync(profile);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TransferStudentAsync(Guid profileId, Guid newClassRoomId)
        {
            var profile = await _unitOfWork.StudentProfiles.GetByIdAsync(profileId);
            if (profile == null) return false;
            profile.ClassRoomId = newClassRoomId;
            await _unitOfWork.StudentProfiles.UpdateAsync(profile);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string[] Errors)> UpdateStudentAsync(StudentDetailDto dto)
        {
            var profile = await _unitOfWork.StudentProfiles.GetByIdAsync(dto.ProfileId);
            if (profile == null) return (false, new[] { "Student not found." });

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

            profile.StudentNumber = dto.StudentNumber;
            profile.NationalNumber = dto.NationalNumber;
            profile.AcademicYearId = dto.AcademicYearId;
            profile.EnrollmentDate = dto.EnrollmentDate;
            profile.ParentName = dto.ParentName;
            profile.ParentPhone = dto.ParentPhone;
            profile.ParentEmail = dto.ParentEmail;
            profile.Address = dto.Address;
            profile.EmergencyContact = dto.EmergencyContact;

            await _unitOfWork.StudentProfiles.UpdateAsync(profile);
            await _unitOfWork.SaveChangesAsync();
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

        private async Task<IEnumerable<StudentDetailDto>> BuildDtos(IEnumerable<StudentProfile> profiles)
        {
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var academicYears = await _unitOfWork.AcademicYears.GetAllAsync();
            var result = new List<StudentDetailDto>();
            foreach (var profile in profiles)
            {
                var user = await _userManager.FindByIdAsync(profile.UserId.ToString());
                if (user == null) continue;
                result.Add(new StudentDetailDto
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
                    StudentNumber = profile.StudentNumber,
                    NationalNumber = profile.NationalNumber,
                    ClassRoomId = profile.ClassRoomId,
                    ClassRoomName = profile.ClassRoomId.HasValue
                        ? classRooms.FirstOrDefault(c => c.Id == profile.ClassRoomId.Value)?.Name ?? ""
                        : "Unassigned",
                    AcademicYearId = profile.AcademicYearId,
                    AcademicYearName = profile.AcademicYearId.HasValue
                        ? academicYears.FirstOrDefault(a => a.Id == profile.AcademicYearId)?.Name ?? ""
                        : "",
                    EnrollmentDate = profile.EnrollmentDate,
                    ParentName = profile.ParentName,
                    ParentPhone = profile.ParentPhone,
                    ParentEmail = profile.ParentEmail,
                    Address = profile.Address,
                    EmergencyContact = profile.EmergencyContact,
                    IsActive = user.IsActive
                });
            }
            return result;
        }
    }
}
