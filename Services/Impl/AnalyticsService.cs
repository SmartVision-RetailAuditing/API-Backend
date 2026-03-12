using ApiBackend.DTOs.AnalyticsDtos;
using ApiBackend.Repositories.Interfaces;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _repo;

        public AnalyticsService(IAnalyticsRepository repo)
        {
            _repo = repo;
        }

        public async Task<AnalyticsDto> GetAnalyticsAsync(int days)
        {
            // days sınırlandırma: 7 / 30 / 90
            var validDays = new[] { 7, 30, 90 };
            if (!validDays.Contains(days)) days = 30;

            return await _repo.GetAnalyticsAsync(days);
        }
    }
}