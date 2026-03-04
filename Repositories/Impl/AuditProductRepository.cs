using ApiBackend.Data;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Repositories.Impl
{
    public class AuditProductRepository : IAuditProductRepository
    {
        private readonly AppDbContext _context;

        public AuditProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AuditProduct?> GetByIdAsync(int id)
        {
            return await _context.AuditProducts.FindAsync(id);
        }

        public async Task<AuditProduct> CreateAsync(AuditProduct product)
        {
            await _context.AuditProducts.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task UpdateAsync(AuditProduct product)
        {
            _context.AuditProducts.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(AuditProduct product)
        {
            _context.AuditProducts.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
