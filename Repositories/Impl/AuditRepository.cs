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

        public async Task<IEnumerable<Audit>> GetAuditsAsync(int pageNumber, int pageSize)
        {
            return await _context.Audits
                .Include(a => a.Products)
                .Include(a => a.Issues)
                .OrderByDescending(a => a.Id) // Sayfalama kaymasını engeller
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Audit?> GetAuditByIdAsync(int id)
        {
            return await _context.Audits
                .Include(a => a.Products)
                .Include(a => a.Issues)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Audit> CreateAuditAsync(Audit audit)
        {
            await _context.Audits.AddAsync(audit);
            await _context.SaveChangesAsync();
            return audit;
        }

        public async Task UpdateAuditAsync(Audit audit)
        {
            _context.Audits.Update(audit);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAuditAsync(Audit audit)
        {
            _context.Audits.Remove(audit);
            await _context.SaveChangesAsync();
        }




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