using ApiBackend.DTOs.AuditDtos;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _auditRepository;

        public AuditService(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public async Task<IEnumerable<AuditDto>> GetAllAuditsAsync(int pageNumber, int pageSize)
        {
            var audits = await _auditRepository.GetAuditsAsync(pageNumber, pageSize);
            return audits.Select(MapToDto);
        }

        public async Task<AuditDto?> GetAuditByIdAsync(int id)
        {
            var audit = await _auditRepository.GetAuditByIdAsync(id);
            if (audit == null) return null;
            return MapToDto(audit);
        }

        public async Task<AuditDto> CreateAuditAsync(CreateAuditDto dto)
        {
            var audit = new Audit
            {
                TaskId = dto.TaskId,
                StoreId = dto.StoreId,
                UserId = dto.UserId,
                ImageUrl = dto.ImageUrl,
                CaptureDate = dto.CaptureDate,
                ComplianceScore = dto.ComplianceScore,
                ShelfSharePercentage = dto.ShelfSharePercentage,
                Status = dto.Status,
                BrandDistributionJson = dto.BrandDistributionJson
            };

            var createdAudit = await _auditRepository.CreateAuditAsync(audit);
            return MapToDto(createdAudit);
        }

        public async Task<bool> UpdateAuditAsync(int id, UpdateAuditDto dto)
        {
            var audit = await _auditRepository.GetAuditByIdAsync(id);
            if (audit == null) return false;

            // Partial Update (Sadece dolu gelenleri güncelle)
            if (dto.ImageUrl != null) audit.ImageUrl = dto.ImageUrl;
            if (dto.CaptureDate.HasValue) audit.CaptureDate = dto.CaptureDate.Value;
            if (dto.ComplianceScore.HasValue) audit.ComplianceScore = dto.ComplianceScore.Value;
            if (dto.ShelfSharePercentage.HasValue) audit.ShelfSharePercentage = dto.ShelfSharePercentage.Value;
            if (dto.Status.HasValue) audit.Status = dto.Status.Value;
            if (dto.BrandDistributionJson != null) audit.BrandDistributionJson = dto.BrandDistributionJson;

            await _auditRepository.UpdateAuditAsync(audit);
            return true;
        }

        public async Task<bool> DeleteAuditAsync(int id)
        {
            var audit = await _auditRepository.GetAuditByIdAsync(id);
            if (audit == null) return false;

            await _auditRepository.DeleteAuditAsync(audit);
            return true;
        }

        // --- HELPER METHOD: Entity'den DTO'ya Çevirici ---
        private AuditDto MapToDto(Audit a)
        {
            return new AuditDto
            {
                Id = a.Id,
                TaskId = a.TaskId,
                StoreId = a.StoreId,
                UserId = a.UserId,
                ImageUrl = a.ImageUrl,
                CaptureDate = a.CaptureDate,
                ComplianceScore = a.ComplianceScore,
                ShelfSharePercentage = a.ShelfSharePercentage,
                Status = a.Status.ToString(),
                BrandDistributionJson = a.BrandDistributionJson,

                Products = a.Products?.Select(p => new AuditProductDto
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
                }).ToList() ?? new List<AuditProductDto>(),

                Issues = a.Issues?.Select(i => new AuditIssueDto
                {
                    Id = i.Id,
                    AuditId = i.AuditId,
                    IssueType = i.IssueType.ToString(),
                    Severity = i.Severity.ToString(),
                    Description = i.Description
                }).ToList() ?? new List<AuditIssueDto>()
            };
        }
    }
}
