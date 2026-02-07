using System.ComponentModel.DataAnnotations;

namespace ApiBackend.Entities
{
    // Rolleri Enum olarak tutuyoruz, kod içinde yönetmesi kolay olsun.
    public enum UserRole
    {
        ADMIN,          // Sistem Yöneticisi
        SUPERVISOR,     // Bölge Sorumlusu (Görev atayan)
        FIELD_WORKER    // Saha Elemanı (Mağazaya giden)
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } // Örn: Ahmet Yılmaz

        [Required]
        [EmailAddress]
        public string Email { get; set; } // Giriş için kullanılacak

        [Required]
        public string PasswordHash { get; set; } // Şifrenin şifrelenmiş hali (BCrypt)

        public UserRole Role { get; set; }

        [MaxLength(50)]
        public string? EmployeeId { get; set; } // Örn: FW-2025-0042 (Mobil Profil ekranı için)

        [MaxLength(20)]
        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true; // Kullanıcıyı silmek yerine pasife alırız

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
    }
}
