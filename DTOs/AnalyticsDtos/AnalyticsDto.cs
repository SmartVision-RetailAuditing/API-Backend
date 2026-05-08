namespace ApiBackend.DTOs.AnalyticsDtos
{
    public class AnalyticsDto
    {
        public AnalyticsKpiDto Kpi { get; set; } = new();
        public List<IssueDistDto> IssueDistribution { get; set; } = new();
        public List<ChainMetricsDto> ChainMetrics { get; set; } = new();
    }
}
