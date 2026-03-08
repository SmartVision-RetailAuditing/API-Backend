namespace ApiBackend.DTOs.StoreDtos
{
    public class StoreDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ChainName { get; set; } = string.Empty;
        public string? Region { get; set; }
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public decimal ComplianceScore { get; set; }
        public string Status { get; set; } = string.Empty;
        public int AuditCount { get; set; }
    }
}