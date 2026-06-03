using Microsoft.EntityFrameworkCore;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Infrastructure.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubjectService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<SubjectDto>> GetAllAsync()
        {
            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            return subjects.Select(s => new SubjectDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description
            });
        }

        public async Task<SubjectDto?> GetByIdAsync(Guid id)
        {
            var s = await _unitOfWork.Subjects.GetByIdAsync(id);
            if (s == null) return null;
            return new SubjectDto { Id = s.Id, Name = s.Name, Description = s.Description };
        }

        public async Task<SubjectDto> CreateAsync(SubjectDto dto)
        {
            var entity = new Subject { Id = Guid.NewGuid(), Name = dto.Name, Description = dto.Description };
            await _unitOfWork.Subjects.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> UpdateAsync(SubjectDto dto)
        {
            var entity = await _unitOfWork.Subjects.GetByIdAsync(dto.Id);
            if (entity == null) return false;
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            await _unitOfWork.Subjects.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.Subjects.GetByIdAsync(id);
            if (entity == null) return false;
            await _unitOfWork.Subjects.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
