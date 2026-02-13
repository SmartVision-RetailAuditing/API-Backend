using ApiBackend.Repositories.Interfaces;
using ApiBackend.Data;
using ApiBackend.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Repositories.Impl
{
    public class StoreRepository : IStoreRepository
    {
        private readonly AppDbContext _context;

        public StoreRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Store>> GetAllStoresAsync(int pageNumber, int pageSize)
        {
            return await _context.Stores
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Store>> GetStoresWithAuditsAsync(int pageNumber, int pageSize)
        {
            // KRİTİK PERFORMANS DÜZELTMESİ:
            // Include(s => s.Audits) sayesinde SQL Join atar. 
            // Tek sorguda (veya optimize 2 sorguda) tüm veriyi çeker. Foreach döngüsüne gerek kalmaz.
            return await _context.Stores
                .Include(s => s.Audits)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Store?> GetStoreByIdAsync(int id)
        {
            // Id ile çekerken de Auditleri getirelim ki detay sayfasında puan hesaplayabilelim
            return await _context.Stores
                .Include(s => s.Audits)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddStoreAsync(Store store)
        {
            await _context.Stores.AddAsync(store);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStoreAsync(Store store)
        {
            _context.Stores.Update(store);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteStoreAsync(Store store)
        {
            _context.Stores.Remove(store);
            await _context.SaveChangesAsync();
        }


    }
}
