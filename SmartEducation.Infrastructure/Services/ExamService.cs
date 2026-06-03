using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;
using SmartEducation.Domain.Enums;

namespace SmartEducation.Infrastructure.Services
{
    public class ExamService : IExamService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExamService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ExamDto>> GetAllAsync()
        {
            var exams = await _unitOfWork.Exams.GetAllAsync();
            var subjects = await _unitOfWork.Subjects.GetAllAsync();
            var classRooms = await _unitOfWork.ClassRooms.GetAllAsync();
            var questions = await _unitOfWork.QuestionBanks.GetAllAsync();

            return exams.Select(e => new ExamDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                SubjectId = e.SubjectId,
                SubjectName = subjects.FirstOrDefault(s => s.Id == e.SubjectId)?.Name ?? "",
                ClassRoomId = e.ClassRoomId,
                ClassRoomName = e.ClassRoomId.HasValue
                    ? classRooms.FirstOrDefault(c => c.Id == e.ClassRoomId)?.Name
                    : null,
                ExamType = e.ExamType,
                ExamDate = e.ExamDate,
                TotalMarks = e.TotalMarks,
                DurationMinutes = e.DurationMinutes,
                QuestionCount = questions.Count(q => q.ExamId == e.Id)
            });
        }

        public async Task<IEnumerable<ExamDto>> GetBySubjectAsync(Guid subjectId)
        {
            var all = await GetAllAsync();
            return all.Where(e => e.SubjectId == subjectId);
        }

        public async Task<ExamDto?> GetByIdAsync(Guid id)
        {
            var e = await _unitOfWork.Exams.GetByIdAsync(id);
            if (e == null) return null;
            var subject = await _unitOfWork.Subjects.GetByIdAsync(e.SubjectId);
            return new ExamDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                SubjectId = e.SubjectId,
                SubjectName = subject?.Name ?? "",
                ClassRoomId = e.ClassRoomId,
                ExamType = e.ExamType,
                ExamDate = e.ExamDate,
                TotalMarks = e.TotalMarks,
                DurationMinutes = e.DurationMinutes
            };
        }

        public async Task<ExamDto> CreateAsync(ExamDto dto)
        {
            var entity = new Exam
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                SubjectId = dto.SubjectId,
                ClassRoomId = dto.ClassRoomId,
                ExamType = dto.ExamType,
                ExamDate = dto.ExamDate,
                TotalMarks = dto.TotalMarks,
                DurationMinutes = dto.DurationMinutes
            };
            await _unitOfWork.Exams.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            dto.Id = entity.Id;
            return dto;
        }

        public async Task<bool> UpdateAsync(ExamDto dto)
        {
            var entity = await _unitOfWork.Exams.GetByIdAsync(dto.Id);
            if (entity == null) return false;
            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.SubjectId = dto.SubjectId;
            entity.ClassRoomId = dto.ClassRoomId;
            entity.ExamType = dto.ExamType;
            entity.ExamDate = dto.ExamDate;
            entity.TotalMarks = dto.TotalMarks;
            entity.DurationMinutes = dto.DurationMinutes;
            await _unitOfWork.Exams.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.Exams.GetByIdAsync(id);
            if (entity == null) return false;
            await _unitOfWork.Exams.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
