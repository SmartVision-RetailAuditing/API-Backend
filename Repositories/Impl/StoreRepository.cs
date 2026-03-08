using ApiBackend.Data;
using ApiBackend.DTOs;
using ApiBackend.DTOs.StoreDtos;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
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

        public async Task<PagedResult<StoreDto>> GetStoresAsync(
            int pageNumber,
            int pageSize,
            string? search = null)
        {
            var query = _context.Stores.AsQueryable();

            // Search filtresi: Name veya ChainName içinde arama
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(s =>
                    s.Name.ToLower().Contains(lower) ||
                    s.ChainName.ToLower().Contains(lower) ||
                    (s.Region != null && s.Region.ToLower().Contains(lower))
                );
            }

            var totalCount = await query.CountAsync();

            // ComplianceScore ve AuditCount DB'de hesaplanıyor (RAM'e entity almıyoruz)
            var data = await query
                .OrderByDescending(s => s.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new StoreDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ChainName = s.ChainName,
                    Region = s.Region,
                    Address = s.Address,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    AuditCount = s.Audits.Count(),
                    ComplianceScore = s.Audits.Any()
                        ? Math.Round(s.Audits.Average(a => a.ComplianceScore), 2)
                        : 0m,
                    Status = !s.Audits.Any() ? "Unknown"
                        : s.Audits.Average(a => a.ComplianceScore) >= 80 ? "Compliant"
                        : s.Audits.Average(a => a.ComplianceScore) >= 60 ? "Warning"
                        : "Non-Compliant"
                })
                .ToListAsync();

            return new PagedResult<StoreDto>
            {
                Data = data,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<Store?> GetStoreByIdAsync(int id)
        {
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