namespace ApiBackend.DTOs.AuditDtos
{
    public class AuditResultDto
    {
        // POST /api/Audits/submit response'u — mobil app'e döner
        public int AuditId { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public decimal ComplianceScore { get; set; }
        public decimal ShelfSharePercentage { get; set; }
        public string Status { get; set; } = string.Empty; // "COMPLIANT" | "WARNING" | "NON_COMPLIANT"
        public DateTime CaptureDate { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public int TotalIssues { get; set; }
    }
}
