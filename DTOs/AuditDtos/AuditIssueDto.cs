namespace ApiBackend.DTOs.AuditDtos
{
    public class AuditIssueDto
    {
        public int Id { get; set; }
        public int AuditId { get; set; }
        public string IssueType { get; set; } // Enum'ın string hali
        public string Severity { get; set; }  // Enum'ın string hali
        public string Description { get; set; }
    }
}
