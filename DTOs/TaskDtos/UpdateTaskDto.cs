using System.ComponentModel.DataAnnotations;
using ApiBackend.Entities;

namespace ApiBackend.DTOs.TaskDtos
{
    public class UpdateTaskDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "StoreId must be greater than 0")]
        public int? StoreId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than 0")]
        public int? UserId { get; set; }

        [EnumDataType(typeof(TaskType), ErrorMessage = "Invalid TaskType value")]
        public TaskType? TaskType { get; set; }

        [EnumDataType(typeof(TaskPriority), ErrorMessage = "Invalid Priority value")]
        public TaskPriority? Priority { get; set; }

        public DateTime? DueDate { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [EnumDataType(typeof(AuditTaskStatus), ErrorMessage = "Invalid Status value")]
        public AuditTaskStatus? Status { get; set; }
    }
}
