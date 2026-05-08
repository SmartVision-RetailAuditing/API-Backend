namespace ApiBackend.DTOs.DashboardDtos
{
    public class RecentIssueDto
    {
        public int Id { get; set; }
        public int AuditId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string IssueType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CaptureDate { get; set; }
    }
}
