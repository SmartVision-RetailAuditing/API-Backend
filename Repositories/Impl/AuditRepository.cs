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
                    PreImageUrl = a.PreImageUrl,
                    PostImageUrl = a.PostImageUrl,
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

        public async Task<PagedResult<MyAuditDto>> GetMyAuditsAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? search = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _context.Audits
                .Include(a => a.Store)
                .Include(a => a.Task)
                .Include(a => a.Products)
                .Include(a => a.Issues)
                .Where(a => a.UserId == userId)  // temel filtre — başka kullanıcı göremesin
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();

                // 1. ÇÖZÜM: C# tarafında arama kelimesiyle eşleşen TaskType enum'larını bul
                var matchingTaskTypes = Enum.GetValues<TaskType>() // TaskType senin gerçek Enum adın olmalı
                    .Where(e => e.ToString().ToLower().Contains(lower))
                    .ToList();

                // SQL'e "Store Name içeriyor mu VEYA TaskType bu listedekilerden biri mi?" diye sor (IN sorgusu)
                query = query.Where(a =>
                    a.Store.Name.ToLower().Contains(lower) ||
                    matchingTaskTypes.Contains(a.Task.TaskType));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<AuditStatus>(status.ToUpper(), out var parsedStatus))
                    query = query.Where(a => a.Status == parsedStatus);
            }

            // 2. ÇÖZÜM: Tarihleri UTC olarak işaretle
            if (startDate.HasValue)
            {
                var utcStartDate = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(a => a.CaptureDate >= utcStartDate);
            }

            if (endDate.HasValue)
            {
                // AddTicks(-1) yerine < operatörü ile ertesi günün gece yarısından küçük olanları alıyoruz
                var utcEndDate = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(a => a.CaptureDate < utcEndDate);
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderByDescending(a => a.CaptureDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new MyAuditDto
                {
                    Id = a.Id,
                    TaskId = a.TaskId,
                    StoreId = a.StoreId,
                    StoreName = a.Store.Name,
                    TaskType = a.Task.TaskType.ToString(),
                    PreImageUrl = a.PreImageUrl,
                    PostImageUrl = a.PostImageUrl,
                    CaptureDate = a.CaptureDate,
                    ComplianceScore = a.ComplianceScore,
                    ShelfSharePercentage = a.ShelfSharePercentage,
                    Status = a.Status.ToString(),
                    BrandDistributionJson = a.BrandDistributionJson,
                    Products = a.Products.Select(p => new MyAuditProductDto
                    {
                        Id = p.Id,
                        AuditId = p.AuditId,
                        ProductName = p.ProductName,
                        ProductCode = p.ProductCode,
                        BrandName = p.BrandName,
                        Volume = p.Volume,
                        Category = p.Category,
                        Price = p.Price,
                        IsEyeLevel = p.IsEyeLevel,
                        ShelfPosition = p.ShelfPosition,
                        BoundingBoxX = p.BoundingBoxX,
                        BoundingBoxY = p.BoundingBoxY,
                        BoundingBoxWidth = p.BoundingBoxWidth,
                        BoundingBoxHeight = p.BoundingBoxHeight,
                        IsManuallyEdited = p.IsManuallyEdited,
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

            return new PagedResult<MyAuditDto>
            {
                Data = data,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = pageNumber,
                PageSize = pageSize
            };
        }
    }
}