using ApiBackend.DTOs.AuditDtos;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    public class AuditProductService : IAuditProductService
    {
        private readonly IAuditProductRepository _productRepository;
        private readonly IAuditRecalculationService _recalculationService;

        public AuditProductService(
            IAuditProductRepository productRepository,
            IAuditRecalculationService recalculationService)
        {
            _productRepository = productRepository;
            _recalculationService = recalculationService;
        }

        public async Task<AuditProductDto> AddProductToAuditAsync(CreateAuditProductDto dto)
        {
            var product = new AuditProduct
            {
                AuditId = dto.AuditId,
                ProductName = dto.ProductName,
                ProductCode = dto.ProductCode,
                BrandName = dto.BrandName,
                Price = dto.Price,
                BoundingBoxX = dto.BoundingBoxX,
                BoundingBoxY = dto.BoundingBoxY,
                BoundingBoxWidth = dto.BoundingBoxWidth,
                BoundingBoxHeight = dto.BoundingBoxHeight,
                ConfidenceScore = dto.ConfidenceScore,
                IsManuallyEdited = true // Web'den manuel eklendiği için direkt true
            };

            var created = await _productRepository.CreateAsync(product);

            // Veritabanından oluşan ID ve diğer tüm alanları DTO'ya mapliyoruz
            return new AuditProductDto
            {
                Id = created.Id,
                AuditId = created.AuditId,
                ProductName = created.ProductName,
                ProductCode = created.ProductCode,
                BrandName = created.BrandName,
                Price = created.Price,
                IsManuallyEdited = created.IsManuallyEdited,
                BoundingBoxX = created.BoundingBoxX,
                BoundingBoxY = created.BoundingBoxY,
                BoundingBoxWidth = created.BoundingBoxWidth,
                BoundingBoxHeight = created.BoundingBoxHeight,
                ConfidenceScore = created.ConfidenceScore
            };
        }

        public async Task<bool> UpdateProductAsync(int id, UpdateAuditProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return false;

            // Partial update
            if (dto.ProductName != null) product.ProductName = dto.ProductName;
            if (dto.ProductCode != null) product.ProductCode = dto.ProductCode;
            if (dto.BrandName != null) product.BrandName = dto.BrandName;
            if (dto.Price.HasValue) product.Price = dto.Price.Value;

            // Supervisor bir değişiklik yaptıysa bunu işaretle
            product.IsManuallyEdited = true;

            await _productRepository.UpdateAsync(product);
            return true;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return false;

            await _productRepository.DeleteAsync(product);
            return true;
        }

        // ── Yeni metod — field worker manuel düzenleme ────────────────────────────

        public async Task<FieldWorkerUpdateResultDto?> FieldWorkerUpdateProductAsync(
            int productId,
            int requestingUserId,
            FieldWorkerUpdateAuditProductDto dto)
        {
            // 1. Ürünü Audit navigation property'siyle birlikte getir
            var product = await _productRepository.GetByIdWithAuditAsync(productId);
            if (product == null) return null; // controller 404 döner

            // 2. Ownership kontrolü — sadece audit'i submit eden field worker düzenleyebilir
            if (product.Audit.UserId != requestingUserId)
                return new FieldWorkerUpdateResultDto { IsForbidden = true }; // controller 403 döner

            // 3. Partial update — null gelen alan olduğu gibi kalır
            if (dto.BrandName != null) product.BrandName = dto.BrandName;
            if (dto.ProductName != null) product.ProductName = dto.ProductName;
            if (dto.ProductCode != null) product.ProductCode = dto.ProductCode;
            if (dto.Volume != null) product.Volume = dto.Volume;
            if (dto.Category != null) product.Category = dto.Category;
            if (dto.Price != null) product.Price = dto.Price;
            if (dto.ShelfPosition != null) product.ShelfPosition = dto.ShelfPosition;
            if (dto.IsEyeLevel != null) product.IsEyeLevel = dto.IsEyeLevel.Value;

            // IsManuallyEdited client'tan alınmıyor — backend set eder
            product.IsManuallyEdited = true;

            await _productRepository.UpdateAsync(product);

            // 4. Audit metriklerini yeniden hesapla (ShelfShare, BrandDist, ComplianceScore, Status)
            await _recalculationService.RecalculateAsync(product.AuditId);

            // 5. Güncel audit metriklerini çek ve dön
            var updatedAudit = await _productRepository.GetAuditMetricsAsync(product.AuditId);

            return new FieldWorkerUpdateResultDto
            {
                IsForbidden = false,
                ProductId = product.Id,
                IsManuallyEdited = product.IsManuallyEdited,
                AuditId = updatedAudit.Id,
                ShelfSharePercentage = updatedAudit.ShelfSharePercentage,
                ComplianceScore = updatedAudit.ComplianceScore,
                Status = updatedAudit.Status.ToString(),
                BrandDistributionJson = updatedAudit.BrandDistributionJson
            };
        }
    }
}
