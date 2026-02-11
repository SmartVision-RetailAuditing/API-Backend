using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiBackend.Entities
{
    public class StoreCompliance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("store_id")]
        public int StoreId { get; set; }

        [Required]
        [Column("store_name")]
        [MaxLength(150)]
        public string StoreName { get; set; }

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

        [Required]
        [Column("region")]
        [MaxLength(50)]
        public string Region { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("compliance")]
        public decimal Compliance { get; set; }

        [Required]
        [Column("shelf_share_percentage")]
        public decimal ShelfPercentage { get; set; }

    }
}

