using ApiBackend.Data;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Repositories.Impl
{
    public class AuditIssueRepository : IAuditIssueRepository
    {
        private readonly AppDbContext _context;

        public AuditIssueRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AuditIssue?> GetByIdAsync(int id)
        {
            return await _context.AuditIssues.FindAsync(id);
        }

        public async Task<AuditIssue> CreateAsync(AuditIssue issue)
        {
            await _context.AuditIssues.AddAsync(issue);
            await _context.SaveChangesAsync();
            return issue;
        }

        public async Task UpdateAsync(AuditIssue issue)
        {
            _context.AuditIssues.Update(issue);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(AuditIssue issue)
        {
            _context.AuditIssues.Remove(issue);
            await _context.SaveChangesAsync();
        }
    }
}
