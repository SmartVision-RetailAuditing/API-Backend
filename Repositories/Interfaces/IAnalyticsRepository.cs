using ApiBackend.DTOs.AnalyticsDtos;

namespace ApiBackend.Repositories.Interfaces
{
    public interface IAnalyticsRepository
    {
        Task<AnalyticsDto> GetAnalyticsAsync(int days);

    }
}
