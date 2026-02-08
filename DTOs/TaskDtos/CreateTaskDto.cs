using System.ComponentModel.DataAnnotations;
using ApiBackend.Entities; // Enumları kullanmak için

namespace ApiBackend.DTOs.TaskDtos
{
    // Web'den Yeni Görev OLUŞTURURKEN Gelecek Veri
    public class CreateTaskDto
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
    }
}
