using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Infrastructure.Services
{
    public class ClassRoomService : IClassRoomService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClassRoomService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ClassRoomDto>> GetAllAsync()
        {
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var grades = await _unitOfWork.Grades.GetAllAsync();
            var students = await _unitOfWork.StudentProfiles.GetAllAsync();

            return classRooms.Select(c =>
            {
                var grade = grades.FirstOrDefault(g => g.Id == c.GradeId);
                return new ClassRoomDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    GradeId = c.GradeId,
                    GradeName = grade?.Name ?? "",
                    StudentCount = students.Count(s => s.ClassRoomId == c.Id)
                };
            });
        }

        public async Task<ClassRoomDto?> GetByIdAsync(Guid id)
        {
            var c = await _unitOfWork.ClassRooms.GetByIdAsync(id);
            if (c == null) return null;
            var grade = await _unitOfWork.Grades.GetByIdAsync(c.GradeId);
            return new ClassRoomDto { Id = c.Id, Name = c.Name, GradeId = c.GradeId, GradeName = grade?.Name ?? "" };
        }

        public async Task<IEnumerable<ClassRoomDto>> GetByGradeAsync(Guid gradeId)
        {
            var all = await GetAllAsync();
            return all.Where(c => c.GradeId == gradeId);
        }

        public async Task<ClassRoomDto> CreateAsync(ClassRoomDto dto)
        {
            var entity = new ClassRoom { Id = Guid.NewGuid(), Name = dto.Name, GradeId = dto.GradeId };
            await _unitOfWork.ClassRooms.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> UpdateAsync(ClassRoomDto dto)
        {
            var entity = await _unitOfWork.ClassRooms.GetByIdAsync(dto.Id);
            if (entity == null) return false;
            entity.Name = dto.Name;
            entity.GradeId = dto.GradeId;
            await _unitOfWork.ClassRooms.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.ClassRooms.GetByIdAsync(id);
            if (entity == null) return false;
            await _unitOfWork.ClassRooms.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
