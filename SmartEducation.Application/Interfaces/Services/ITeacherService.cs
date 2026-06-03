using SmartEducation.Application.DTOs;
using SmartEducation.Domain.Entities;

namespace SmartEducation.Application.Interfaces.Services
{
    public interface ITeacherService
    {
        Task<TeacherDashboardDto> GetDashboardAsync(Guid userId);
        Task<TeacherProfile?> GetProfileByUserIdAsync(Guid userId);

        // Lesson Plans
        Task<IEnumerable<LessonPlanDto>> GetLessonPlansAsync(Guid teacherProfileId);
        Task<LessonPlanDto?> GetLessonPlanByIdAsync(Guid id, Guid teacherProfileId);
        Task<LessonPlanDto> CreateLessonPlanAsync(LessonPlanDto dto);
        Task<bool> UpdateLessonPlanAsync(LessonPlanDto dto, Guid teacherProfileId);
        Task<bool> DeleteLessonPlanAsync(Guid id, Guid teacherProfileId);

        // Attendance
        Task<IEnumerable<AttendanceSessionDto>> GetAttendanceSessionsAsync(Guid teacherProfileId);
        Task<AttendanceSessionDto> CreateAttendanceSessionAsync(Guid teacherProfileId, Guid classRoomId, Guid subjectId, DateTime date);
        Task MarkAttendanceAsync(Guid sessionId, Dictionary<Guid, bool> studentAttendance);
        Task MarkAttendanceWithStatusAsync(Guid sessionId, Dictionary<Guid, string> studentStatuses);

        // Students in teacher's classes
        Task<IEnumerable<StudentSummaryDto>> GetMyStudentsAsync(Guid teacherProfileId);

        // Assignments (teacher side)
        Task<IEnumerable<AssignmentDto>> GetMyAssignmentsAsync(Guid teacherProfileId);
        Task<AssignmentDto> CreateAssignmentAsync(AssignmentDto dto, Guid teacherProfileId);
        Task<bool> DeleteAssignmentAsync(Guid id, Guid teacherProfileId);
    }
}
