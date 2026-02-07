using System.ComponentModel.DataAnnotations;

namespace ApiBackend.Entities
{
    public class Store
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } // Örn: Migros MM Kadıköy

        [MaxLength(50)]
        public string ChainName { get; set; } // Örn: Migros, Şok, A101 (Raporlama için önemli)

        // Harita ve Navigasyon için koordinatlar
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string Address { get; set; }

        [MaxLength(50)]
        public string? Region { get; set; } // Örn: Marmara, Kadıköy Bölgesi

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
