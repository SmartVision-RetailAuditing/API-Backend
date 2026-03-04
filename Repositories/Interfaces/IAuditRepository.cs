using ApiBackend.Entities;

namespace ApiBackend.Repositories.Interfaces
{
    public interface IAuditRepository
    {

        Task<IEnumerable<Audit>> GetAuditsAsync(int pageNumber, int pageSize);
        Task<Audit?> GetAuditByIdAsync(int id);
        Task<Audit> CreateAuditAsync(Audit audit);
        Task UpdateAuditAsync(Audit audit);
        Task DeleteAuditAsync(Audit audit);


        // Kullanıcının yaptığı tüm denetimleri puanlarıyla getirir
        Task<IEnumerable<Audit>> GetAuditsByUserIdAsync(int userId);
        Task<IEnumerable<Audit>> GetAuditsByStoreIdAsync(int storeId);
    }
}
