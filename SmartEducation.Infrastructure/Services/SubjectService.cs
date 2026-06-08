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
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            return subjects.Select(s => MapToDto(s, classRooms));
        }

        public async Task<IEnumerable<SubjectDto>> GetByClassRoomAsync(Guid classRoomId)
        {
            var all = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            return all.Where(s => s.ClassRoomId == classRoomId).Select(s => MapToDto(s, classRooms));
        }



        public async Task<SubjectDto?> GetByIdAsync(Guid id)
        {
            var s = await _unitOfWork.Subjects.GetByIdAsync(id);
            if (s == null) return null;
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            return MapToDto(s, classRooms);
        }

        public async Task<SubjectDto> CreateAsync(SubjectDto dto)
        {
            var entity = new Subject
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                ClassRoomId = dto.ClassRoomId == Guid.Empty ? null : dto.ClassRoomId
            };
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
            entity.ClassRoomId = dto.ClassRoomId == Guid.Empty ? null : dto.ClassRoomId;
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

        private static SubjectDto MapToDto(Subject s, IEnumerable<ClassRoom> classRooms) => new()
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            ClassRoomId = s.ClassRoomId,
            ClassRoomName = s.ClassRoomId.HasValue
                ? classRooms.FirstOrDefault(c => c.Id == s.ClassRoomId.Value)?.Name ?? ""
                : ""
        };
    }
}
