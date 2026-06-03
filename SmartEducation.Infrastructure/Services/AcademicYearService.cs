using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Infrastructure.Services
{
    public class AcademicYearService : IAcademicYearService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AcademicYearService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<AcademicYearDto>> GetAllAsync()
        {
            var years = await _unitOfWork.AcademicYears.GetAllAsync();
            return years.Select(y => new AcademicYearDto
            {
                Id = y.Id,
                Name = y.Name,
                StartDate = y.StartDate,
                EndDate = y.EndDate,
                IsActive = y.StartDate <= DateTime.Now && y.EndDate >= DateTime.Now
            });
        }

        public async Task<AcademicYearDto?> GetByIdAsync(Guid id)
        {
            var y = await _unitOfWork.AcademicYears.GetByIdAsync(id);
            if (y == null) return null;
            return new AcademicYearDto { Id = y.Id, Name = y.Name, StartDate = y.StartDate, EndDate = y.EndDate };
        }

        public async Task<AcademicYearDto> CreateAsync(AcademicYearDto dto)
        {
            var entity = new AcademicYear
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };
            await _unitOfWork.AcademicYears.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> UpdateAsync(AcademicYearDto dto)
        {
            var entity = await _unitOfWork.AcademicYears.GetByIdAsync(dto.Id);
            if (entity == null) return false;
            entity.Name = dto.Name;
            entity.StartDate = dto.StartDate;
            entity.EndDate = dto.EndDate;
            await _unitOfWork.AcademicYears.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.AcademicYears.GetByIdAsync(id);
            if (entity == null) return false;
            await _unitOfWork.AcademicYears.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
