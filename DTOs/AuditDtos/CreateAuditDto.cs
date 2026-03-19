using ApiBackend.Entities;
using System.ComponentModel.DataAnnotations;

namespace ApiBackend.DTOs.AuditDtos
{
    public class CreateAuditDto
    {
        [Required]
        public int TaskId { get; set; }

        [Required]
        public int StoreId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string? PreImageUrl { get; set; } // Sütun Required ama formdan string null/boş gelebileceği için validation'a takılsın diye ? bırakılır
        
        [Required]
        public string? PostImageUrl { get; set; } // Sütun Required ama formdan string null/boş gelebileceği için validation'a takılsın diye ? bırakılır


        [Required]
        public DateTime CaptureDate { get; set; }

        [Required]
        public decimal ComplianceScore { get; set; }

        [Required]
        public decimal ShelfSharePercentage { get; set; }

        [Required]
        public AuditStatus Status { get; set; }

        public string? BrandDistributionJson { get; set; }
    }
}
