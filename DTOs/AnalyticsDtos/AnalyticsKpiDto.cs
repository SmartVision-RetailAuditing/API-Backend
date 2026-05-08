namespace ApiBackend.DTOs.AnalyticsDtos
{
    public class AnalyticsKpiDto
    {
        public decimal AvgCompliance { get; set; }
        public int TotalAudits { get; set; }
        public int CompliantStores { get; set; }  // compliance_score >= 80 olan unique store
        public int TotalIssues { get; set; }
    }
}
