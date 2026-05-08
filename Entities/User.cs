using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiBackend.Entities
{
    public enum UserRole
    {
        ADMIN,
        SUPERVISOR,
        FIELD_WORKER
    }

    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("full_name")]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [Column("email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Required]
        [Column("role")]
        public UserRole Role { get; set; }

        [Column("employee_id")]
        [MaxLength(50)]
        public string? EmployeeId { get; set; }

        [Column("phone")]
        [MaxLength(20)]
        public string? Phone { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }
    }
}
