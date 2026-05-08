namespace ApiBackend.DTOs
{
    public class NotificationDto
    {
        public string Id { get; set; } = string.Empty; // "{type}-{entityId}" — frontend key için
        public string Type { get; set; } = string.Empty; // NEW_AUDIT | CRITICAL_ISSUE | NEW_USER | ROLE_CHANGED | TASK_ASSIGNED | OVERDUE_TASK
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? NavigateTo { get; set; } // tıklanınca gidilecek route
        public bool IsRead { get; set; } = false;
    }
}