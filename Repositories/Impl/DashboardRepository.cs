using ApiBackend.Data;
using ApiBackend.DTOs.DashboardDtos;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Repositories.Impl
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;

        public DashboardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalStoresAsync()
        {
            return await _context.Stores.CountAsync();
        }

        public async Task<decimal> GetAverageComplianceAsync()
        {
            var hasAudits = await _context.Audits.AnyAsync();
            return hasAudits
                ? (decimal)await _context.Audits.AverageAsync(a => a.ComplianceScore)
                : 0m;
        }

        public async Task<int> GetPendingTasksCountAsync()
        {
            return await _context.Tasks
                .CountAsync(t => t.Status == AuditTaskStatus.PENDING);
        }

        public async Task<int> GetCriticalIssuesCountAsync()
        {
            return await _context.AuditIssues
                .CountAsync(i => i.Severity == IssueSeverity.CRITICAL);
        }

        public async Task<List<RecentIssueDto>> GetRecentIssuesAsync(int count = 10)
        {
            return await _context.AuditIssues
                .Include(i => i.Audit)
                    .ThenInclude(a => a.Store)
                .OrderByDescending(i => i.Audit.CaptureDate)
                .Take(count)
                .Select(i => new RecentIssueDto
                {
                    Id = i.Id,
                    AuditId = i.AuditId,
                    StoreName = i.Audit.Store.Name,
                    Location = i.Audit.Store.Address,
                    IssueType = i.IssueType.ToString(),
                    Severity = i.Severity.ToString(),
                    Description = i.Description,
                    CaptureDate = i.Audit.CaptureDate
                })
                .ToListAsync();
        }

        public async Task<List<RecentAuditDto>> GetRecentAuditsAsync(int count = 10)
        {
            return await _context.Audits
                .Include(a => a.Store)
                .Include(a => a.User)
                .Include(a => a.Task)
                .OrderByDescending(a => a.CaptureDate)
                .Take(count)
                .Select(a => new RecentAuditDto
                {
                    Id = a.Id,
                    StoreName = a.Store.Name,
                    AuditorName = a.User.FullName,
                    TaskType = a.Task.TaskType.ToString(),
                    ComplianceScore = a.ComplianceScore,
                    Status = a.Status.ToString(),
                    Location = a.Store.Address,
                    CaptureDate = a.CaptureDate
                })
                .ToListAsync();
        }
    }
}