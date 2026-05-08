using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using System.Text.Json.Serialization; // Döngüsel referansı önlemek için

namespace ApiBackend.Entities
{
    [Table("stores")]
    public class Store
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(150)]
        public string Name { get; set; }

        [Required]
        [Column("chain_name")]
        [MaxLength(50)]
        public string ChainName { get; set; }

        [Required]
        [Column("latitude")]
        public double Latitude { get; set; }

        [Required]
        [Column("longitude")]
        public double Longitude { get; set; }

        [Required]
        [Column("address")]
        public string Address { get; set; }

        [Column("region")]
        [MaxLength(50)]
        public string? Region { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // İLİŞKİ: Bir mağazanın birden çok denetimi olabilir.
        // JsonIgnore: API direk Store dönerse Auditleri içine gömmesin diye (DTO kullanıyoruz ama önlem)
        // Store Service'deki foreach döngüsü performans katilidir. Bunu çözmek için Entity Framework'ün Join yapabilmesi lazım. Bunun için Store entity'sine Audits listesini eklememiz gerek.
        [JsonIgnore]
        public ICollection<Audit> Audits { get; set; }
    }
}
