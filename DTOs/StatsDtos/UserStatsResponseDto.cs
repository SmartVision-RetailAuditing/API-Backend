namespace ApiBackend.DTOs.StatsDtos
{
    public class UserStatsResponseDto
    {
        public WeeklyTasksStatsDto WeeklyTasks { get; set; }
        public PerformanceStatsDto Performance { get; set; }
    }
}
