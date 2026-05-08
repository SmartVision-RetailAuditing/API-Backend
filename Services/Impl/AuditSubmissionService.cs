using ApiBackend.Data; // Kendi DbContext namespace'ini yaz
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
        private readonly IShelfComplianceRuleService _ruleEngine;

        public AuditSubmissionService(
            ICloudStorageService storageService,
            IAIVisionService aiVisionService,
            AiResponseMapper mapper,
            AppDbContext context,
            IShelfComplianceRuleService ruleEngine)
        {
            _storageService = storageService;
            _aiVisionService = aiVisionService;
            _mapper = mapper;
            _context = context;
            _ruleEngine = ruleEngine;
        }
        
        public async Task<AuditResultDto> ProcessAuditAsync(IFormFile image, int taskId, int userId)
        {
            // 1. Task kontrolü
            var task = await _context.Tasks.Include(t => t.Store).FirstOrDefaultAsync(t => t.Id == taskId) 
                       ?? throw new KeyNotFoundException($"Task {taskId} bulunamadı.");

            // 2. Fotoğraf Yükle
            var PreImageUrl = await _storageService.UploadImageAsync(image);

            // 3. AI Servisine Yolla (Ham veri al)
            var aiResult = await _aiVisionService.AnalyzeShelfAsync(image);

            // 4. Mapper (DTO -> Entity Çevirisi)
            var products = _mapper.MapToEntity(aiResult.Products);

            // 5. Kural Motoru (İş Zekası ve İhlaller)
            var (issues, score, shelfShare, brandDist) = _ruleEngine.EvaluateRules(products);

            // 6. DB Entity'sini hazırla
            var audit = new Audit
            {
                TaskId = taskId,
                StoreId = task.StoreId,
                UserId = userId,
                PreImageUrl = PreImageUrl,
                PostImageUrl = aiResult.PostImageAzureUrl,
                CaptureDate = DateTime.UtcNow,
                ComplianceScore = score,
                ShelfSharePercentage = shelfShare,
                BrandDistributionJson = brandDist,
                Status = score >= 80 ? AuditStatus.COMPLIANT : score >= 60 ? AuditStatus.WARNING : AuditStatus.NON_COMPLIANT,
                Products = products,
                Issues = issues
            };

            // 7. DB'ye yaz ve Task'ı kapat
            _context.Audits.Add(audit);
            task.Status = AuditTaskStatus.COMPLETED;
            task.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // 8. Sonucu dön
            return new AuditResultDto
            {
                AuditId = audit.Id,
                StoreId = audit.StoreId,
                StoreName = task.Store.Name,
                ComplianceScore = audit.ComplianceScore,
                ShelfSharePercentage = audit.ShelfSharePercentage,
                Status = audit.Status.ToString(),
                CaptureDate = audit.CaptureDate,
                TotalProducts = products.Count,
                TotalIssues = issues.Count,
            };
        }
    }
}