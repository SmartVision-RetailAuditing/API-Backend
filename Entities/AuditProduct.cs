using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ApiBackend.Entities
{
    [Table("audit_products")]
    public class AuditProduct
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("audit_id")]
        public int AuditId { get; set; }

        [ForeignKey(nameof(AuditId))]
        [JsonIgnore]
        public Audit Audit { get; set; } = null!;

        [Required]
        [Column("product_name")]
        [MaxLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [Column("product_code")]
        [MaxLength(50)]
        public string? ProductCode { get; set; }

        [Required]
        [Column("brand_name")]
        [MaxLength(50)]
        public string BrandName { get; set; } = string.Empty;

        // Yeni — "1000 ML", "500 ML"
        [Column("volume")]
        [MaxLength(30)]
        public string? Volume { get; set; }

        // Yeni — "SÜT", "YOĞURT"
        [Column("category")]
        [MaxLength(50)]
        public string? Category { get; set; }

        [Column("price", TypeName = "decimal(10, 2)")]
        public decimal? Price { get; set; }

        // Yeni — göz hizasında mı? (toplantı notu: "yerden yüksekliği, göz hizası")
        [Required]
        [Column("is_eye_level")]
        public bool IsEyeLevel { get; set; } = false;

        // Yeni — kaçıncı raf (1 = en üst)
        [Column("shelf_position")]
        public int? ShelfPosition { get; set; }

        [Required]
        [Column("is_manually_edited")]
        public bool IsManuallyEdited { get; set; } = false;

        [Required]
        [Column("bounding_box_x")]
        public int BoundingBoxX { get; set; }

        [Required]
        [Column("bounding_box_y")]
        public int BoundingBoxY { get; set; }

        [Required]
        [Column("bounding_box_width")]
        public int BoundingBoxWidth { get; set; }

        [Required]
        [Column("bounding_box_height")]
        public int BoundingBoxHeight { get; set; }

        [Required]
        [Column("confidence_score")]
        public double ConfidenceScore { get; set; }
    }
}