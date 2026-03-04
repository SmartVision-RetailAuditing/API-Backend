using ApiBackend.Entities;
using System.ComponentModel.DataAnnotations;

namespace ApiBackend.DTOs.AuditDtos
{
    public class CreateAuditIssueDto
    {
        [Required]
        public int AuditId { get; set; }

        [Required]
        public IssueType IssueType { get; set; }

        [Required]
        public IssueSeverity Severity { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
