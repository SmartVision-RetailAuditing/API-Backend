// Burası "Yapay Zeka Ne Gördü?" tablosudur. Fotoğraftaki her bir süt kutusu, her bir etiket buraya bir satır olarak kaydedilir.
// Amaç: İleride "Rakip X'in fiyatı neydi?" raporunu çekebilmek.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiBackend.Entities
{
    public class AuditProduct
    {
        [Key]
        public int Id { get; set; }

        public int AuditId { get; set; } // Hangi fotoğrafa ait?

        [Required]
        [MaxLength(100)]
        public string ProductName { get; set; } // Örn: Pınar Süt 1L

        [MaxLength(50)]
        public string? ProductCode { get; set; } // Barkod/SKU

        // DEĞİŞİKLİK 1: "Biz vs Diğerleri" yerine Marka Adı
        [Required]
        [MaxLength(50)]
        public string BrandName { get; set; } // "Pınar", "Sütaş", "Torku", "Sek"

        // Toplantı notu: "Fiyat okuma için OCR kullanılıyor"
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? Price { get; set; }

        // DEĞİŞİKLİK 2: Manuel Müdahale Takibi
        // Eğer saha çalışanı fiyatı veya ismi elle düzeltirse bu true olacak.
        public bool IsManuallyEdited { get; set; } = false;

        // Koordinatlar (Mobil UI'da kutucuğa tıklamak için şart)
        public int BoundingBoxX { get; set; }
        public int BoundingBoxY { get; set; }
        public int BoundingBoxWidth { get; set; }
        public int BoundingBoxHeight { get; set; }

        public double ConfidenceScore { get; set; } // AI Güven Skoru
    }
}
