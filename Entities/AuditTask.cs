// Hangi saha çalışanı hangi mağazaya, ne zaman ve neden gidecek?

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiBackend.Entities
{
    public enum AuditTaskStatus { PENDING, IN_PROGRESS, COMPLETED }
    public enum TaskPriority { LOW, MEDIUM, HIGH }

    // Toplantı notlarına göre görev tipleri
    public enum TaskType
    {
        SHELF_AUDIT,            // Standart Raf Denetimi
        PRICE_CHECK,            // Fiyat Kontrolü (Rakip analizi için)
        PANORAMA,               // Geniş raf çekimi
        PLANOGRAM_COMPLIANCE    // Dizilim kontrolü
    }

    public class AuditTask
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } // Görevi yapacak personel

        [ForeignKey("Store")]
        public int StoreId { get; set; }
        public Store Store { get; set; } // Gidilecek mağaza

        public TaskType TaskType { get; set; }
        public TaskPriority Priority { get; set; }

        public AuditTaskStatus Status { get; set; } = AuditTaskStatus.PENDING;

        public DateTime DueDate { get; set; } // Son yapılması gereken tarih
        public DateTime? CompletedAt { get; set; } // Ne zaman tamamlandı?

        public string? Description { get; set; } // Örn: "Süt reyonuna dikkat et"
    }
}
