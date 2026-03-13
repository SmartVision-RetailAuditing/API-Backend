using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ApiBackend.Entities
{
    // HIGH eklendi — sözleşme ihlali var ama henüz kritik değil durumu için
    public enum IssueSeverity { LOW, MEDIUM, HIGH, CRITICAL }

    public enum IssueType
    {
        MISSING_PRODUCT,
        WRONG_PRICE,
        LOW_SHELF_SHARE,
        WRONG_SHELF_POSITION,
        PLANOGRAM_MISMATCH
    }

    [Table("audit_issues")]
    public class AuditIssue
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
        [Column("issue_type")]
        public IssueType IssueType { get; set; }

        [Required]
        [Column("severity")]
        public IssueSeverity Severity { get; set; }

        [Required]
        [Column("description")]
        public string Description { get; set; } = string.Empty;
    }
}