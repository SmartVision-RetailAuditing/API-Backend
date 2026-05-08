namespace ApiBackend.Services.Interfaces
{
    /// <summary>
    /// Field worker manuel düzenleme yaptıktan sonra audit metriklerini
    /// yeniden hesaplayan servisin kontratı.
    /// </summary>
    public interface IAuditRecalculationService
    {
        /// <summary>
        /// Verilen auditId için ShelfShare, BrandDistribution,
        /// ComplianceScore ve Status'u yeniden hesaplayıp kaydeder.
        /// </summary>
        Task RecalculateAsync(int auditId);
    }
}