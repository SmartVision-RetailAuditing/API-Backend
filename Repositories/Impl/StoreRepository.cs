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

        public async Task<IEnumerable<Store>> GetAllStoresAsync()
        {
            return await _context.Stores.ToListAsync();
        }

        public async Task<Store?> GetStoreByIdAsync(int id)
        {
            return await _context.Stores.FindAsync(id);
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
