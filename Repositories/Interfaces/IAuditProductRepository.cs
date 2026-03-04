using ApiBackend.Entities;

namespace ApiBackend.Repositories.Interfaces
{
    public interface IAuditProductRepository
    {
        Task<AuditProduct?> GetByIdAsync(int id);
        Task<AuditProduct> CreateAsync(AuditProduct product);
        Task UpdateAsync(AuditProduct product);
        Task DeleteAsync(AuditProduct product);
    }
}
