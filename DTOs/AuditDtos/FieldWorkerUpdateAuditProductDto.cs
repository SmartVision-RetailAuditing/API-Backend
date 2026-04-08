namespace ApiBackend.DTOs.AuditDtos
{
    /// <summary>
    /// PATCH /api/AuditProducts/{id} — field worker'ın gönderdiği request body.
    ///
    /// Null gönderilen alan güncellenmez (partial update).
    /// IsManuallyEdited client'tan alınmaz — backend otomatik true set eder.
    ///
    /// Mevcut UpdateAuditProductDto'dan ayrı tutuldu:
    ///   UpdateAuditProductDto → ADMIN/SUPERVISOR (web, PUT)
    ///   Bu DTO               → FIELD_WORKER     (mobil, PATCH)
    /// Fark: Volume, Category, ShelfPosition, IsEyeLevel eklendi.
    /// </summary>
    public class FieldWorkerUpdateAuditProductDto
    {
        public string? BrandName { get; set; }  // "Teksut" → "PINAR"
        public string? ProductName { get; set; }  // "Bilinmeyen Ürün" → "Süt Tam Yağlı 1L"
        public string? ProductCode { get; set; }  // "153100000"
        public string? Volume { get; set; }  // "1000 ML", "500 ML"
        public string? Category { get; set; }  // "SÜT", "YOĞURT"
        public decimal? Price { get; set; }
        public int? ShelfPosition { get; set; }  // 1 = en üst raf
        public bool? IsEyeLevel { get; set; }
    }

    /// <summary>
    /// AuditProductService.FieldWorkerUpdateProductAsync iç dönüş modeli.
    ///
    /// IsForbidden = true  → başkasının audit'i → controller 403 döner
    /// IsForbidden = false → başarılı           → controller 200 + bu dto döner
    /// null döndüğünde     → ürün bulunamadı    → controller 404 döner
    ///
    /// Mobil bu response'la UI'ı anında güncelleyebilir — ekstra GET atmak gerekmez.
    /// </summary>
    public class FieldWorkerUpdateResultDto
    {
        public bool IsForbidden { get; set; }
        public int ProductId { get; set; }
        public bool IsManuallyEdited { get; set; }
        public int AuditId { get; set; }
        public decimal ShelfSharePercentage { get; set; }
        public decimal ComplianceScore { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? BrandDistributionJson { get; set; }
    }
}