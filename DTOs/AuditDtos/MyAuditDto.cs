namespace ApiBackend.DTOs.AuditDtos
{
    public class MyAuditDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string TaskType { get; set; } = string.Empty;
        public string? PreImageUrl { get; set; }
        public string? PostImageUrl { get; set; }
        public DateTime CaptureDate { get; set; }
        public decimal ComplianceScore { get; set; }
        public decimal ShelfSharePercentage { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? BrandDistributionJson { get; set; }
        public List<MyAuditProductDto> Products { get; set; } = new();
        public List<AuditIssueDto> Issues { get; set; } = new();
    }
}