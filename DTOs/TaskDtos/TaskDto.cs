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
        public string StoreName { get; set; } // "Migros MM Kadıköy"
        public string StoreAddress { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Görev Detayları
        public string TaskType { get; set; } // "Shelf Audit"
        public string Priority { get; set; } // "High"
        public string Status { get; set; }   // "Pending"
        public DateTime DueDate { get; set; }
        public string? Description { get; set; }

        // Atanan Kişi (Web tarafı için lazım olabilir)
        public int AssigneeId { get; set; }
        public string AssigneeName { get; set; }
    }
}
