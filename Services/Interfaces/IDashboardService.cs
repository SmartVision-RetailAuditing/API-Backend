using ApiBackend.DTOs.DashboardDtos;

namespace ApiBackend.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardKpiDto> GetKpisAsync();
        Task<List<RecentIssueDto>> GetRecentIssuesAsync();
        Task<List<RecentAuditDto>> GetRecentAuditsAsync();
    }
}
