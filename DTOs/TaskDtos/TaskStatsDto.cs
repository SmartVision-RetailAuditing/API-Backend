namespace ApiBackend.DTOs.TaskDtos
{
    public class TaskStatsDto
    {
        public int TotalActive { get; set; }  // Tüm aktif task sayısı
        public int Pending { get; set; }      // Atanmamış / bekleyen
        public int InProgress { get; set; }   // Şu an devam eden
        public int CompletedThisWeek { get; set; } // Bu hafta tamamlanan
        public int Unassigned { get; set; }  // UserId == null
    }
}
