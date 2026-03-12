namespace ApiBackend.DTOs.AnalyticsDtos
{
    public class ChainMetricsDto
    {
        public string ChainName { get; set; } = string.Empty;
        public decimal AvgCompliance { get; set; }
        public int TotalAudits { get; set; }
        public decimal Trend { get; set; }   // dönem ilk yarısı vs ikinci yarısı farkı
        public string Status { get; set; } = string.Empty; // Excellent / Good / Needs Attention
    }
}
