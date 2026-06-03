using SmartEducation.Application.DTOs;

namespace SmartEducation.Application.Interfaces.Services
{
    public interface ICurriculumPlanService
    {
        Task<IEnumerable<CurriculumPlanDto>> GetByTeacherAsync(Guid teacherProfileId);
        Task<CurriculumPlanDto?> GetByIdAsync(Guid id);
        Task<CurriculumPlanDto> GeneratePlanAsync(GenerateCurriculumPlanRequest request);
        Task<bool> MarkItemCompletedAsync(Guid itemId, bool completed);
        Task<bool> UpdateItemAsync(CurriculumPlanItemDto dto);
        Task<bool> DeletePlanAsync(Guid id);
        Task<CurriculumPlanDto> GetProgressAsync(Guid planId);
    }
}
