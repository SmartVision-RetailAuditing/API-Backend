using ApiBackend.Entities;

namespace ApiBackend.DTOs.TaskDtos
{
    public class UpdateTaskDto
    {
        public int? StoreId { get; set; }
        public int? UserId { get; set; }
        public TaskType? TaskType { get; set; }
        public TaskPriority? Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Description { get; set; }
        public AuditTaskStatus? Status { get; set; }
    }
}