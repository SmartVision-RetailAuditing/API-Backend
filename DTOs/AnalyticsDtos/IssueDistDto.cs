namespace ApiBackend.DTOs.AnalyticsDtos
{
    public class IssueDistDto
    {
        public string IssueType { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
