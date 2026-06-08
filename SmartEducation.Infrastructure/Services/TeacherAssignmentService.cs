using Microsoft.AspNetCore.Identity;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Infrastructure.Services
{
    public class TeacherAssignmentService : ITeacherAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeacherAssignmentService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IEnumerable<TeacherAssignmentDetailDto>> GetAllAsync()
        {
            var assignments = await _unitOfWork.TeacherAssignments.GetAllAsync();
            return await BuildDtos(assignments);
        }

        public async Task<IEnumerable<TeacherAssignmentDetailDto>> GetByClassRoomAsync(Guid classRoomId)
        {
            var all = await _unitOfWork.TeacherAssignments.GetAllAsync();
            return await BuildDtos(all.Where(a => a.ClassRoomId == classRoomId));
        }

        public async Task<TeacherAssignmentDetailDto?> GetByIdAsync(Guid id)
        {
            var assignment = await _unitOfWork.TeacherAssignments.GetByIdAsync(id);
            if (assignment == null) return null;
            var dtos = await BuildDtos(new[] { assignment });
            return dtos.FirstOrDefault();
        }

        public async Task<(bool Success, string Error)> AssignTeacherAsync(TeacherAssignmentDetailDto dto)
        {
            var all = await _unitOfWork.TeacherAssignments.GetAllAsync();
            var duplicate = all.Any(a =>
                a.TeacherId == dto.TeacherId &&
                a.SubjectId == dto.SubjectId &&
                a.ClassRoomId == dto.ClassRoomId &&
                a.IsActive);

            if (duplicate)
                return (false, "This teacher is already assigned to this subject in this class.");

            await _unitOfWork.TeacherAssignments.AddAsync(new TeacherAssignment
            {
                Id = Guid.NewGuid(),
                TeacherId = dto.TeacherId,
                SubjectId = dto.SubjectId,
                ClassRoomId = dto.ClassRoomId,
                AcademicYearId = dto.AcademicYearId,
                IsActive = true
            });
            await _unitOfWork.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<bool> RemoveAssignmentAsync(Guid id)
        {
            var entity = await _unitOfWork.TeacherAssignments.GetByIdAsync(id);
            if (entity == null) return false;
            await _unitOfWork.TeacherAssignments.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveAsync(Guid id)
        {
            var entity = await _unitOfWork.TeacherAssignments.GetByIdAsync(id);
            if (entity == null) return false;
            entity.IsActive = !entity.IsActive;
            await _unitOfWork.TeacherAssignments.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private async Task<IEnumerable<TeacherAssignmentDetailDto>> BuildDtos(IEnumerable<TeacherAssignment> assignments)
        {
            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var teachers = await _unitOfWork.TeacherProfiles.GetAllAsync();
            var academicYears = await _unitOfWork.AcademicYears.GetAllAsync();

            var result = new List<TeacherAssignmentDetailDto>();
            foreach (var a in assignments)
            {
                var teacher = teachers.FirstOrDefault(t => t.Id == a.TeacherId);
                var teacherUser = teacher != null
                    ? await _userManager.FindByIdAsync(teacher.UserId.ToString())
                    : null;

                result.Add(new TeacherAssignmentDetailDto
                {
                    Id = a.Id,
                    TeacherId = a.TeacherId,
                    TeacherName = teacherUser != null ? $"{teacherUser.FirstName} {teacherUser.LastName}" : "N/A",
                    TeacherEmail = teacherUser?.Email ?? "",
                    SubjectId = a.SubjectId,
                    SubjectName = subjects.FirstOrDefault(s => s.Id == a.SubjectId)?.Name ?? "",
                    ClassRoomId = a.ClassRoomId,
                    ClassRoomName = classRooms.FirstOrDefault(c => c.Id == a.ClassRoomId)?.Name ?? "",
                    AcademicYearId = a.AcademicYearId,
                    AcademicYearName = a.AcademicYearId.HasValue
                        ? academicYears.FirstOrDefault(y => y.Id == a.AcademicYearId)?.Name ?? ""
                        : "",
                    IsActive = a.IsActive
                });
            }
            return result;
        }
    }
}
