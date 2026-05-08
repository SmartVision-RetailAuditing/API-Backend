using ApiBackend.DTOs.DashboardDtos;
using ApiBackend.Repositories.Interfaces;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<DashboardKpiDto> GetKpisAsync()
        {
            return new DashboardKpiDto
            {
                TotalStores = await _dashboardRepository.GetTotalStoresAsync(),
                AverageCompliance = Math.Round(await _dashboardRepository.GetAverageComplianceAsync(), 1),
                PendingTasks = await _dashboardRepository.GetPendingTasksCountAsync(),
                CriticalIssues = await _dashboardRepository.GetCriticalIssuesCountAsync()
            };
        }

        public async Task<List<RecentIssueDto>> GetRecentIssuesAsync()
        {
            return await _dashboardRepository.GetRecentIssuesAsync(10);
        }

        public async Task<List<RecentAuditDto>> GetRecentAuditsAsync()
        {
            return await _dashboardRepository.GetRecentAuditsAsync(10);
        }
    }
}
