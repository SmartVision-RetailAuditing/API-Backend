namespace ApiBackend.DTOs.DashboardDtos
{
    public class DashboardKpiDto
    {
        public int TotalStores { get; set; }
        public decimal AverageCompliance { get; set; }
        public int PendingTasks { get; set; }
        public int CriticalIssues { get; set; }
    }
}
