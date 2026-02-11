using ApiBackend.Entities;

namespace ApiBackend.Repositories.Interfaces
{
    public interface IAuditRepository
    {
        // Kullanıcının yaptığı tüm denetimleri puanlarıyla getirir
        Task<IEnumerable<Audit>> GetAuditsByUserIdAsync(int userId);
        Task<IEnumerable<Audit>> GetAuditsByStoreIdAsync(int storeId);
    }
}
