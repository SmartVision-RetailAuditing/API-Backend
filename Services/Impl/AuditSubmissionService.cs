using ApiBackend.Data;
using ApiBackend.DTOs.AuditDtos;
using ApiBackend.Entities;
using ApiBackend.Mappers;
using ApiBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Services.Impl
{
    public class AuditSubmissionService : IAuditSubmissionService
    {
        private readonly ICloudStorageService _storageService;
        private readonly IAIVisionService _aiVisionService;
        private readonly AiResponseMapper _mapper;
        private readonly AppDbContext _context;

        public AuditSubmissionService(
            ICloudStorageService storageService,
            IAIVisionService aiVisionService,
            AiResponseMapper mapper,
            AppDbContext context)
        {
            _storageService = storageService;
            _aiVisionService = aiVisionService;
            _mapper = mapper;
            _context = context;
        }

        public async Task<AuditResultDto> ProcessAuditAsync(IFormFile image, int taskId, int userId)
        {
            // ── 1. Task'ı doğrula ────────────────────────────────────────────
            var task = await _context.Tasks
                .Include(t => t.Store)
                .FirstOrDefaultAsync(t => t.Id == taskId)
                ?? throw new KeyNotFoundException($"Task {taskId} bulunamadı.");

            if (task.UserId != userId)
                throw new UnauthorizedAccessException("Bu task size ait değil.");

            if (task.Status == AuditTaskStatus.COMPLETED)
                throw new InvalidOperationException("Bu task zaten tamamlanmış.");

            // ── 2. Fotoğrafı Blob Storage'a yükle ───────────────────────────
            var imageUrl = await _storageService.UploadImageAsync(image);

            // ── 3. AI servisine gönder ───────────────────────────────────────
            var aiResult = await _aiVisionService.AnalyzeShelfAsync(imageUrl);

            // ── 4. AI response → Entity dönüşümü ────────────────────────────
            var (audit, products, issues) = _mapper.Map(
                aiResult,
                taskId,
                task.StoreId,
                userId,
                imageUrl);

            // ── 5. DB'ye tek transaction'da yaz ─────────────────────────────
            audit.Products = products;
            audit.Issues = issues;

            _context.Audits.Add(audit);

            task.Status = AuditTaskStatus.COMPLETED;
            task.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(); // tek SaveChanges — atomik

            // ── 6. Sonucu döndür ─────────────────────────────────────────────
            return new AuditResultDto
            {
                AuditId = audit.Id,
                StoreId = audit.StoreId,
                StoreName = task.Store.Name,
                ComplianceScore = audit.ComplianceScore,
                ShelfSharePercentage = audit.ShelfSharePercentage,
                Status = audit.Status.ToString(),
                CaptureDate = audit.CaptureDate,
                ImageUrl = audit.ImageUrl!,
                TotalProducts = products.Count,
                TotalIssues = issues.Count,
            };
        }
    }
}