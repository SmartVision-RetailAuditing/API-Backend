namespace ApiBackend.DTOs.AuditDtos
{
    public class MyAuditProductDto
    {
        public int Id { get; set; }
        public int AuditId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductCode { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string? Volume { get; set; }
        public string? Category { get; set; }
        public decimal? Price { get; set; }
        public bool IsEyeLevel { get; set; }
        public int? ShelfPosition { get; set; }
        public bool IsManuallyEdited { get; set; }
        public double ConfidenceScore { get; set; }
    }
}