using ApiBackend.DTOs.AuditDtos;

namespace ApiBackend.Services.Interfaces
{
    public interface IAuditService
    {
        Task<IEnumerable<AuditDto>> GetAllAuditsAsync(int pageNumber, int pageSize);
        Task<AuditDto?> GetAuditByIdAsync(int id);
        Task<AuditDto> CreateAuditAsync(CreateAuditDto createDto);
        Task<bool> UpdateAuditAsync(int id, UpdateAuditDto updateDto);
        Task<bool> DeleteAuditAsync(int id);
    }
}
