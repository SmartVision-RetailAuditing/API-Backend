using System.ComponentModel.DataAnnotations;
using ApiBackend.Entities; // Enumları kullanmak için

namespace ApiBackend.DTOs.TaskDtos
{
    // Web'den Yeni Görev OLUŞTURURKEN Gelecek Veri
    public class CreateTaskDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "StoreId must be greater than 0")]
        public int StoreId { get; set; }

        public int? UserId { get; set; } // null = atanmamış (unassigned)

        [Required]
        [EnumDataType(typeof(TaskType), ErrorMessage = "Invalid task type")]
        public TaskType TaskType { get; set; } // Enum olarak gelecek (0, 1, 2...)

        [Required]
        [EnumDataType(typeof(TaskPriority), ErrorMessage = "Invalid priority")]
        public TaskPriority Priority { get; set; }

        [Required(ErrorMessage = "DueDate is required")]
        public DateTime DueDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
