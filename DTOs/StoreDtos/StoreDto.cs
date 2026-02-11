using System.ComponentModel.DataAnnotations;
using ApiBackend.Entities; // Enumları kullanmak için

namespace ApiBackend.DTOs.TaskDtos
{
    public class StoreDto
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; } // 
        public string ChainName { get; set; } //  TODO: Enum or lookup table
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } // "Shelf Audit"
        public string Region { get; set; } // TODO: Enum
        public DateTime CreatedAt { get; set; }
    }
}
