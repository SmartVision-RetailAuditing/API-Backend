namespace ApiBackend.DTOs.StatsDtos
{
    public class PerformanceStatsDto
    {
        public int CompletionRate { get; set; } // % Yüzde
        public int AverageScore { get; set; }   // Puan
        public int TotalStoreVisits { get; set; }
    }
}
