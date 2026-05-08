namespace ApiBackend.DTOs.DashboardDtos
{
    public class RecentAuditDto
    {
        public int Id { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string AuditorName { get; set; } = string.Empty;
        public string TaskType { get; set; } = string.Empty;
        public decimal ComplianceScore { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime CaptureDate { get; set; }
    }
}
