using ApiBackend.DTOs.AuditDtos;
using ApiBackend.Entities;

namespace ApiBackend.Mappers
{
    public class AiResponseMapper
    {
        public List<AuditProduct> MapToEntity(List<AiProductDto>? dtos)
        {
            if (dtos == null || !dtos.Any()) 
                return new List<AuditProduct>();

            return dtos.Select(p => new AuditProduct
            {
                ProductName = p.ProductName,
                ProductCode = p.ProductCode,
                BrandName = p.BrandName,
                Volume = p.Volume,
                Category = p.Category,
                Price = p.Price,
                ConfidenceScore = p.ConfidenceScore,
                IsEyeLevel = p.IsEyeLevel,
                ShelfPosition = p.ShelfPosition,
                BoundingBoxX = p.BoundingBox.X,
                BoundingBoxY = p.BoundingBox.Y,
                BoundingBoxWidth = p.BoundingBox.Width,
                BoundingBoxHeight = p.BoundingBox.Height,
                IsManuallyEdited = false
            }).ToList();
        }
    }
}