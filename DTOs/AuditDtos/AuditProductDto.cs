namespace ApiBackend.DTOs.AuditDtos
{
    public class AuditProductDto
    {
        public int Id { get; set; }
        public int AuditId { get; set; }
        public string ProductName { get; set; }
        public string? ProductCode { get; set; }
        public string BrandName { get; set; }
        public decimal? Price { get; set; }
        public bool IsManuallyEdited { get; set; }
        public int BoundingBoxX { get; set; }
        public int BoundingBoxY { get; set; }
        public int BoundingBoxWidth { get; set; }
        public int BoundingBoxHeight { get; set; }
        public double ConfidenceScore { get; set; }
    }
}
