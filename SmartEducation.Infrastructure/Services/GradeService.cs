using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Infrastructure.Services
{
    public class GradeService : IGradeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GradeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<GradeDto>> GetAllAsync()
        {
            var grades = await _unitOfWork.Grades.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            return grades.Select(g => new GradeDto
            {
                Id = g.Id,
                Name = g.Name,
                ClassRoomCount = classRooms.Count(c => c.GradeId == g.Id)
            });
        }

        public async Task<GradeDto?> GetByIdAsync(Guid id)
        {
            var g = await _unitOfWork.Grades.GetByIdAsync(id);
            if (g == null) return null;
            return new GradeDto { Id = g.Id, Name = g.Name };
        }

        public async Task<GradeDto> CreateAsync(GradeDto dto)
        {
            var entity = new Grade { Id = Guid.NewGuid(), Name = dto.Name };
            await _unitOfWork.Grades.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> UpdateAsync(GradeDto dto)
        {
            var entity = await _unitOfWork.Grades.GetByIdAsync(dto.Id);
            if (entity == null) return false;
            entity.Name = dto.Name;
            await _unitOfWork.Grades.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.Grades.GetByIdAsync(id);
            if (entity == null) return false;
            await _unitOfWork.Grades.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
