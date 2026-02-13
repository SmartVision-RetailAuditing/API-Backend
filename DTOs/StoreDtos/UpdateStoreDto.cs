using System.ComponentModel.DataAnnotations;


namespace ApiBackend.DTOs.StoreDtos
{
    public class UpdateStoreDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string ChainName { get; set; } // Migros, CarrefourSA, Metro
        public string? Region { get; set; }  // Region might be optional
        [Required]
        public string Address { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
    }
}
