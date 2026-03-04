using ApiBackend.DTOs.AuditDtos;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    public class AuditIssueService : IAuditIssueService
    {
        private readonly IAuditIssueRepository _issueRepository;

        public AuditIssueService(IAuditIssueRepository issueRepository)
        {
            _issueRepository = issueRepository;
        }

        public async Task<AuditIssueDto> AddIssueToAuditAsync(CreateAuditIssueDto dto)
        {
            var issue = new AuditIssue
            {
                AuditId = dto.AuditId,
                IssueType = dto.IssueType,
                Severity = dto.Severity,
                Description = dto.Description
            };

            var created = await _issueRepository.CreateAsync(issue);

            return new AuditIssueDto
            {
                Id = created.Id,
                AuditId = created.AuditId,
                IssueType = created.IssueType.ToString(),
                Severity = created.Severity.ToString(),
                Description = created.Description
            };
        }

        public async Task<bool> DeleteIssueAsync(int id)
        {
            var issue = await _issueRepository.GetByIdAsync(id);
            if (issue == null) return false;

            await _issueRepository.DeleteAsync(issue);
            return true;
        }
    }
}
