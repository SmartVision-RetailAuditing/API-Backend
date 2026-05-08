using System.ComponentModel.DataAnnotations;

namespace ApiBackend.DTOs.AuditDtos
{
    public class CreateAuditProductDto
    {
        [Required]
        public int AuditId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ProductName { get; set; }

        [MaxLength(50)]
        public string? ProductCode { get; set; }

        [Required]
        [MaxLength(50)]
        public string BrandName { get; set; }

        public decimal? Price { get; set; }

        [Required]
        public int BoundingBoxX { get; set; }

        [Required]
        public int BoundingBoxY { get; set; }

        [Required]
        public int BoundingBoxWidth { get; set; }

        [Required]
        public int BoundingBoxHeight { get; set; }

        [Required]
        public double ConfidenceScore { get; set; }
    }
}
