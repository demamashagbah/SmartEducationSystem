using SmartEducation.Application.DTOs;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Application.Interfaces.Services
{
    public interface IStudentService
    {
        Task<StudentDashboardDto> GetDashboardAsync(Guid userId);
        Task<StudentProfile?> GetProfileByUserIdAsync(Guid userId);
        Task<IEnumerable<StudentSubjectSummaryDto>> GetSubjectsAsync(Guid studentProfileId);
        Task<IEnumerable<AssignmentDto>> GetAssignmentsAsync(Guid studentProfileId);
        Task<bool> SubmitAssignmentAsync(Guid assignmentId, Guid studentProfileId, string? notes);
        Task<IEnumerable<ExamDto>> GetUpcomingExamsAsync(Guid studentProfileId);
    }
}
