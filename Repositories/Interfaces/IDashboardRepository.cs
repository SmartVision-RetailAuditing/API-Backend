namespace ApiBackend.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        Task<int> GetTotalStoresAsync();
        Task<decimal> GetAverageComplianceAsync();
        Task<int> GetPendingTasksCountAsync();
        Task<int> GetCriticalIssuesCountAsync();
        Task<List<DTOs.DashboardDtos.RecentIssueDto>> GetRecentIssuesAsync(int count = 10);
        Task<List<DTOs.DashboardDtos.RecentAuditDto>> GetRecentAuditsAsync(int count = 10);
    }
}
