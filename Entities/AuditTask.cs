
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiBackend.Entities
{
    public enum AuditTaskStatus { PENDING, IN_PROGRESS, COMPLETED }
    public enum TaskPriority { LOW, MEDIUM, HIGH }


    public enum TaskType
    {
        SHELF_AUDIT,
        PRICE_CHECK,
        PANORAMA,
        PLANOGRAM_COMPLIANCE
    }

    [Table("audit_tasks")]
    public class AuditTask
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        // FK → User
        [Column("user_id")]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        public User User { get; set; } = null!;

        // FK → Store
        [Column("store_id")]
        [ForeignKey(nameof(Store))]
        public int StoreId { get; set; }

        public Store Store { get; set; } = null!;

        [Required]
        [Column("task_type")]
        public TaskType TaskType { get; set; }

        [Required]
        [Column("priority")]
        public TaskPriority Priority { get; set; }

        [Required]
        [Column("status")]
        public AuditTaskStatus Status { get; set; } = AuditTaskStatus.PENDING;

        [Required]
        [Column("due_date")]
        public DateTime DueDate { get; set; }

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [Column("description")]
        public string? Description { get; set; }
    }
}
