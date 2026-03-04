using ApiBackend.Entities;

namespace ApiBackend.Repositories.Interfaces
{
    public interface IAuditIssueRepository
    {
        Task<AuditIssue?> GetByIdAsync(int id);
        Task<AuditIssue> CreateAsync(AuditIssue issue);
        Task UpdateAsync(AuditIssue issue); // Issue düzeltilirse diye update koyalım
        Task DeleteAsync(AuditIssue issue);
    }
}
