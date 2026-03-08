namespace ApiBackend.DTOs.AuditDtos
{
    public class AuditDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int StoreId { get; set; }
        public int UserId { get; set; }

        // Enriched fields (navigation property'lerden geliyor)
        public string StoreName { get; set; } = string.Empty;
        public string AuditorName { get; set; } = string.Empty;
        public string TaskType { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
        public DateTime CaptureDate { get; set; }
        public decimal ComplianceScore { get; set; }
        public decimal ShelfSharePercentage { get; set; }
        public string Status { get; set; } // Enum'ın string hali (örn: "COMPLIANT")
        public string? BrandDistributionJson { get; set; }

        public List<AuditProductDto> Products { get; set; } = new();
        public List<AuditIssueDto> Issues { get; set; } = new();
    }
}
