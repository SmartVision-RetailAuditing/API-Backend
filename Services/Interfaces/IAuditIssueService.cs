using ApiBackend.DTOs.AuditDtos;

namespace ApiBackend.Services.Interfaces
{
    public interface IAuditIssueService
    {
        Task<AuditIssueDto> AddIssueToAuditAsync(CreateAuditIssueDto createDto);
        Task<bool> DeleteIssueAsync(int id);
    }
}
