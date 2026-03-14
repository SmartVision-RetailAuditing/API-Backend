using ApiBackend.Data;
using ApiBackend.DTOs;
using ApiBackend.DTOs.AuditDtos;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Repositories.Impl
{
    public class AuditRepository : IAuditRepository
    {
        private readonly AppDbContext _context;
        public AuditRepository(AppDbContext context) { _context = context; }

        public async Task<PagedResult<AuditDto>> GetAuditsAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            string? status = null,
            int? storeId = null)          // ← YENİ
        {
            var query = _context.Audits
                .Include(a => a.Store)
                .Include(a => a.User)
                .Include(a => a.Task)
                .Include(a => a.Products)
                .Include(a => a.Issues)
                .AsQueryable();

            // StoreId filtresi — Store detail sayfasından gelince sadece o mağazanın auditleri
            if (storeId.HasValue)
                query = query.Where(a => a.StoreId == storeId.Value);

            // Search filtresi
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(a =>
                    a.Store.Name.ToLower().Contains(lower) ||
                    a.User.FullName.ToLower().Contains(lower));
            }

            // Status filtresi
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<AuditStatus>(status.ToUpper(), out var parsedStatus))
                    query = query.Where(a => a.Status == parsedStatus);
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderByDescending(a => a.CaptureDate)   // Yeniden eskiye
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditDto
                {
                    Id = a.Id,
                    TaskId = a.TaskId,
                    StoreId = a.StoreId,
                    UserId = a.UserId,
                    StoreName = a.Store.Name,
                    AuditorName = a.User.FullName,
                    TaskType = a.Task.TaskType.ToString(),
                    ImageUrl = a.ImageUrl,
                    CaptureDate = a.CaptureDate,
                    ComplianceScore = a.ComplianceScore,
                    ShelfSharePercentage = a.ShelfSharePercentage,
                    Status = a.Status.ToString(),
                    BrandDistributionJson = a.BrandDistributionJson,
                    Products = a.Products.Select(p => new AuditProductDto
                    {
                        Id = p.Id,
                        AuditId = p.AuditId,
                        ProductName = p.ProductName,
                        ProductCode = p.ProductCode,
                        BrandName = p.BrandName,
                        Price = p.Price,
                        IsManuallyEdited = p.IsManuallyEdited,
                        BoundingBoxX = p.BoundingBoxX,
                        BoundingBoxY = p.BoundingBoxY,
                        BoundingBoxWidth = p.BoundingBoxWidth,
                        BoundingBoxHeight = p.BoundingBoxHeight,
                        ConfidenceScore = p.ConfidenceScore
                    }).ToList(),
                    Issues = a.Issues.Select(i => new AuditIssueDto
                    {
                        Id = i.Id,
                        AuditId = i.AuditId,
                        IssueType = i.IssueType.ToString(),
                        Severity = i.Severity.ToString(),
                        Description = i.Description
                    }).ToList()
                })
                .ToListAsync();

            return new PagedResult<AuditDto>
            {
                Data = data,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<Audit?> GetAuditByIdAsync(int id)
        {
            return await _context.Audits
                .Include(a => a.Store)
                .Include(a => a.User)
                .Include(a => a.Task)
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