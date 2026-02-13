using ApiBackend.Entities;
using System.ComponentModel.DataAnnotations;

namespace ApiBackend.DTOs.TaskDtos
{
    public class UpdateTaskDto
    {
        [Required]
        public int StoreId { get; set; }

        [Required]
        public int UserId { get; set; } // Kime atıyorsun?

        [Required]
        public TaskType TaskType { get; set; } // Enum olarak gelecek (0, 1, 2...)

        [Required]
        public TaskPriority Priority { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public string? Description { get; set; }
        public AuditTaskStatus Status { get; set; }
    }
}
