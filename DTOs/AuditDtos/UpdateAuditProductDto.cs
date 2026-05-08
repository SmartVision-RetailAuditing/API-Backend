using System.ComponentModel.DataAnnotations;

namespace ApiBackend.DTOs.AuditDtos
{
    public class UpdateAuditProductDto
    {
        [MaxLength(100)]
        public string? ProductName { get; set; }

        [MaxLength(50)]
        public string? ProductCode { get; set; }

        [MaxLength(50)]
        public string? BrandName { get; set; }

        public decimal? Price { get; set; }

        // Supervisor fiyatı vb. web üzerinden değiştirirse diye bu alanı gönderiyoruz
        public bool? IsManuallyEdited { get; set; }
    }
}
