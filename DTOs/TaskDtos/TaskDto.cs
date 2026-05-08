using System.ComponentModel.DataAnnotations;
using ApiBackend.Entities; // Enumları kullanmak için

namespace ApiBackend.DTOs.TaskDtos
{
    // Mobilde ve Web Listesinde GÖSTERİLECEK Veri
    public class TaskDto
    {
        public int Id { get; set; }

        // Mağaza Bilgileri (Flattening - Düzleştirme yapıyoruz ki frontend rahat etsin)
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty; // "Migros MM Kadıköy"
        public string StoreAddress { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Görev Detayları
        public string TaskType { get; set; } = string.Empty; // "Shelf Audit"
        public string Priority { get; set; } = string.Empty; // "High"
        public string Status { get; set; } = string.Empty;   // "Pending"
        public DateTime DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }  // null = henüz tamamlanmadı
        public string? Description { get; set; }

        // Atanan Kişi (Web tarafı için lazım olabilir)
        public int AssigneeId { get; set; }
        public string AssigneeName { get; set; } = string.Empty;

        // Linked audit — null ise henüz audit oluşturulmamış
        public int? AuditId { get; set; }
    }
}
