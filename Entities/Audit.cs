// Bir mağaza ziyaretinin Genel Karnesi. Puan kaç? Kaç ürün var?

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiBackend.Entities
{
    public enum AuditStatus { COMPLIANT, WARNING, NON_COMPLIANT }
    public class Audit
    {
        [Key]
        public int Id { get; set; }

        public int TaskId { get; set; } // Hangi görevin sonucu?
        public int StoreId { get; set; }
        public int UserId { get; set; }

        public string? ImageUrl { get; set; } // Çekilen fotoğrafın S3 linki

        public DateTime CaptureDate { get; set; } = DateTime.UtcNow;

        // KPI: Genel Uyumluluk Skoru (0-100)
        public decimal ComplianceScore { get; set; }

        // KPI: Raf Payı (Toplantı notu: "10 üründen 4'ü benim %40")
        public decimal ShelfSharePercentage { get; set; }

        public AuditStatus Status { get; set; } // Yeşil, Sarı, Kırmızı durumu

        // AI Analiz Özetleri
        // DEĞİŞİKLİK: Marka dağılımını JSON olarak tutabiliriz.
        // Örn: { "Pınar": 10, "Sütaş": 5, "Sek": 3 }
        // Postgres JSONB desteği sayesinde bunu raporlamak çok kolaydır.
        [Column(TypeName = "jsonb")]
        public string? BrandDistributionJson { get; set; }

        // Detay tablolarına bağlantılar
        public List<AuditProduct> Products { get; set; } = new List<AuditProduct>();
        public List<AuditIssue> Issues { get; set; } = new List<AuditIssue>();
    }
}
