using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace ApiBackend.Entities
{
    public enum AuditStatus { COMPLIANT, WARNING, NON_COMPLIANT }
    [Table("audits")]
    public class Audit
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("task_id")]
        public int TaskId { get; set; }

        [ForeignKey(nameof(TaskId))]
        [JsonIgnore]
        public AuditTask Task { get; set; } = null!;

        [Required]
        [Column("store_id")]
        public int StoreId { get; set; }

        [ForeignKey(nameof(StoreId))]
        [JsonIgnore]
        public Store Store { get; set; } = null!;

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public User User { get; set; } = null!;

        
        [Column("pre_image_url")]
        public string? PreImageUrl { get; set; }
        
        
        [Column("post_image_url")]
        public string? PostImageUrl { get; set; }

        [Required]
        [Column("capture_date")]
        public DateTime CaptureDate { get; set; } = DateTime.UtcNow;


        [Required]
        [Column("compliance_score")]
        public decimal ComplianceScore { get; set; }


        [Required]
        [Column("shelf_share_percentage")]
        public decimal ShelfSharePercentage { get; set; }

        [Required]
        [Column("status")]
        public AuditStatus Status { get; set; }

        [Column("brand_distrubution_json", TypeName = "jsonb")]
        public string? BrandDistributionJson { get; set; }

        [Required]
        [Column("products")]
        public List<AuditProduct> Products { get; set; } = new List<AuditProduct>();

        [Required]
        [Column("issues")]
        public List<AuditIssue> Issues { get; set; } = new List<AuditIssue>();
    }
}
