using ApiBackend.DTOs;
using ApiBackend.DTOs.AuditDtos;

namespace ApiBackend.Services.Interfaces
{
    public interface IAuditService
    {
        Task<PagedResult<AuditDto>> GetAllAuditsAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            string? status = null,
            int? storeId = null
        );
        Task<AuditDto?> GetAuditByIdAsync(int id);
        Task<AuditDto> CreateAuditAsync(CreateAuditDto createDto);
        Task<bool> UpdateAuditAsync(int id, UpdateAuditDto updateDto);
        Task<bool> DeleteAuditAsync(int id);

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
