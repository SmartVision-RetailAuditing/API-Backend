using ApiBackend.DTOs.AuditDtos;

namespace ApiBackend.Services.Interfaces
{
    public interface IAuditProductService
    {
        Task<AuditProductDto> AddProductToAuditAsync(CreateAuditProductDto createDto);
        Task<bool> UpdateProductAsync(int id, UpdateAuditProductDto updateDto);
        Task<bool> DeleteProductAsync(int id);

        // ── Yeni metod — field worker manuel düzenleme ────────────────────────────
        /// <summary>
        /// Dönüş:
        ///   null                       → ürün bulunamadı (controller 404)
        ///   result.IsForbidden = true  → başkasının audit'i (controller 403)
        ///   result.IsForbidden = false → başarılı, metrikleri dolu (controller 200)
        /// </summary>
        Task<FieldWorkerUpdateResultDto?> FieldWorkerUpdateProductAsync(
            int productId,
            int requestingUserId,
            FieldWorkerUpdateAuditProductDto dto);
    }
}
