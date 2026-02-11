using ApiBackend.Data;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Repositories.Impl
{
    public class AuditRepository : IAuditRepository
    {
        private readonly AppDbContext _context;
        public AuditRepository(AppDbContext context) { _context = context; }

        public async Task<IEnumerable<Audit>> GetAuditsByUserIdAsync(int userId)
        {
            return await _context.Audits
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Audit>> GetAuditsByStoreIdAsync(int storeId)
        {
            return await _context.Audits
                .Where(a => a.StoreId == storeId)
                .ToListAsync();
        }
    }
}