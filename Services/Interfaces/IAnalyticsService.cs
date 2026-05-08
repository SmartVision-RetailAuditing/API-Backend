using ApiBackend.DTOs.AnalyticsDtos;

namespace ApiBackend.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<AnalyticsDto> GetAnalyticsAsync(int days);

    }
}
