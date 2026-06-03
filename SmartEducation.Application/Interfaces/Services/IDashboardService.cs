using SmartEducation.Application.DTOs.Dashboard;

namespace SmartEducation.Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<DashboardStatisticsDto> GetAdminStatisticsAsync();
        Task<AdminAnalyticsDto> GetAdminAnalyticsAsync();
    }
}
