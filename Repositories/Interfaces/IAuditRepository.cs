using ApiBackend.Entities;
using ApiBackend.DTOs;
using ApiBackend.DTOs.AuditDtos;

namespace ApiBackend.Repositories.Interfaces
{
    public interface IAuditRepository
    {
        //IQueryable projection ile direkt DTO döner — RAM'e entity almaz
        Task<PagedResult<AuditDto>> GetAuditsAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            string? status = null,
            int? storeId = null
        );

        Task<Audit?> GetAuditByIdAsync(int id);
        Task<Audit> CreateAuditAsync(Audit audit);
        Task UpdateAuditAsync(Audit audit);
        Task DeleteAuditAsync(Audit audit);


        // Kullanıcının yaptığı tüm denetimleri puanlarıyla getirir
        Task<IEnumerable<Audit>> GetAuditsByUserIdAsync(int userId);
        Task<IEnumerable<Audit>> GetAuditsByStoreIdAsync(int storeId);


        Task<PagedResult<MyAuditDto>> GetMyAuditsAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? search = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null
        );
    }
}
