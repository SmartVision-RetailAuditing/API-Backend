using ApiBackend.Entities;
using System.ComponentModel.DataAnnotations;

namespace ApiBackend.DTOs.AuditDtos
{
    public class UpdateAuditDtos
    {
        public string? ImageUrl { get; set; }
        public DateTime? CaptureDate { get; set; }
        public decimal? ComplianceScore { get; set; }
        public decimal? ShelfSharePercentage { get; set; }
        public AuditStatus? Status { get; set; }
        public string? BrandDistributionJson { get; set; }
    }
}
