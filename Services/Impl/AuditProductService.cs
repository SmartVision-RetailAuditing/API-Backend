using ApiBackend.DTOs.AuditDtos;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    public class AuditProductService : IAuditProductService
    {
        private readonly IAuditProductRepository _productRepository;

        public AuditProductService(IAuditProductRepository productRepository)
        {
            _productRepository = productRepository;
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
    }
}
