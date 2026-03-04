using ApiBackend.Entities;

namespace ApiBackend.DTOs.AuditDtos
{
    public class UpdateAuditIssueDto
    {
        public IssueType? IssueType { get; set; }
        public IssueSeverity? Severity { get; set; }
        public string? Description { get; set; }
    }
}
