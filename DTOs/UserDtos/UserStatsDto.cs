namespace ApiBackend.DTOs.UserDtos
{
    public class UserStatsDto
    {
        public int TotalTasks { get; set; }       // "12 Toplam"
        public int CompletedTasks { get; set; }   // "8 Tamamlandı"
        public int PendingTasks { get; set; }     // "4 Bekliyor"
        public int TotalStoreVisits { get; set; } // "127 Ziyaret Edilen Market"
        public int AverageScore { get; set; }     // "85 Ortalama Puan"
        public int CompletionRate { get; set; }   // "92% Görev Tamamlama Oranı"
    }
}
