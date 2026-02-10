using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Required]
        [Column("product_name")]
        [MaxLength(100)]
        public string ProductName { get; set; }

        [Column("product_code")]
        [MaxLength(50)]
        public string? ProductCode { get; set; }

        [Required]
        [Column("brand_name")]
        [MaxLength(50)]
        public string BrandName { get; set; }

        [Column("price", TypeName = "decimal(10, 2)")]
        public decimal? Price { get; set; }

        [Required]
        [Column("is_manually_edited")]
        public bool IsManuallyEdited { get; set; } = false;

        [Required]
        [Column("boundind_box_x")]
        public int BoundingBoxX { get; set; }

        [Required]
        [Column("boundind_box_y")]
        public int BoundingBoxY { get; set; }

        [Required]
        [Column("boundind_box_width")]

        public int BoundingBoxWidth { get; set; }
        [Required]
        [Column("boundind_box_height")]
        public int BoundingBoxHeight { get; set; }

        [Required]
        [Column("confidence_score")]
        public double ConfidenceScore { get; set; }
    }
}
