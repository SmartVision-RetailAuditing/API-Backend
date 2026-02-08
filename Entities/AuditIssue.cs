// Burası "Sorun Ne?" tablosudur. Mobil arayüzdeki "Kırmızı Uyarı Kutucukları" buradaki verilerden oluşur.

using System.ComponentModel.DataAnnotations;

namespace ApiBackend.Entities
{
    public enum IssueSeverity { LOW, MEDIUM, CRITICAL } // Uyarı rengi için (Sarı, Turuncu, Kırmızı)
    public enum IssueType
    {
        MISSING_PRODUCT,        // Ürün yok (Stok bitmiş)
        WRONG_PRICE,            // Fiyat yanlış
        LOW_SHELF_SHARE,        // Raf payı hedefin altında
        WRONG_SHELF_POSITION,   // Yanlış yere konmuş
        PLANOGRAM_MISMATCH      // Dizilim sırası yanlış
    }
    public class AuditIssue
    {
        [Key]
        public int Id { get; set; }

        public int AuditId { get; set; }

        public IssueType IssueType { get; set; } // Hata türü

        public IssueSeverity Severity { get; set; } // Önem derecesi

        // Mobil ekranda yazacak detaylı açıklama
        // Örn: "Coca Cola 330ml yanlış rafta (Göz seviyesinde olmalı)"
        public string Description { get; set; }
    }
}
