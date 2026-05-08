using ApiBackend.Entities;

namespace ApiBackend.Repositories.Interfaces
{
    public interface IAuditProductRepository
    {
        Task<AuditProduct?> GetByIdAsync(int id);
        Task<AuditProduct> CreateAsync(AuditProduct product);
        Task UpdateAsync(AuditProduct product);
        Task DeleteAsync(AuditProduct product);

        // ── Yeni metodlar — field worker özelliği için ────────────────────────────

        /// <summary>
        /// Ownership kontrolü için Audit navigation property'siyle birlikte getirir.
        /// Servis katmanı product.Audit.UserId ile kimin audit'i olduğunu kontrol eder.
        /// </summary>
        Task<AuditProduct?> GetByIdWithAuditAsync(int id);

        /// <summary>
        /// RecalculationService kaydettikten sonra servis katmanının güncel
        /// audit metriklerini (ShelfShare, ComplianceScore, Status, BrandDist)
        /// okuyup controller'a döndürmesi için kullanılır.
        /// </summary>
        Task<Audit> GetAuditMetricsAsync(int auditId);
    }
}
