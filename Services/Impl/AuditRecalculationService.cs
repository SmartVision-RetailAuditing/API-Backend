using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    public class AuditRecalculationService : IAuditRecalculationService
    {
        // "Pınar", "pınar", "PINAR" → hepsi bu sabit ile eşleşmeli
        // Türkçe locale ile ToUpper: "ı" → "I", "i" → "İ"
        private const string OwnBrand = "PINAR";

        // Türkçe locale — OrdinalIgnoreCase "ı"/"I" farkını göremez, bu yüzden
        // normalize edip == ile karşılaştırıyoruz
        private static readonly CultureInfo TrCulture = new("tr-TR");

        // BrandDistributionJson'da "İÇİM", "SÜTAŞ" gibi Türkçe karakterleri
        // \u0130 gibi escape etmeden düz UTF-8 olarak yaz
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly IAuditRepository _auditRepository;

        public AuditRecalculationService(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        // Tüm brand isimlerini Türkçe locale ile büyük harfe normalize eder.
        // "Pınar" → "PINAR" | "pinar" → "PINAR" | "İçim" → "İÇİM"
        private static string Normalize(string brandName) =>
            brandName.Trim().ToUpper(TrCulture);

        public async Task RecalculateAsync(int auditId)
        {
            var audit = await _auditRepository.GetAuditByIdAsync(auditId)
                ?? throw new KeyNotFoundException($"Audit {auditId} bulunamadı.");

            var products = audit.Products.ToList();

            // ── 1. Shelf Share ────────────────────────────────────────────────────
            audit.ShelfSharePercentage = products.Count > 0
                ? Math.Round(
                    (decimal)products.Count(p => Normalize(p.BrandName) == OwnBrand)
                    / products.Count * 100, 1)
                : 0m;

            // ── 2. Brand Distribution ─────────────────────────────────────────────
            // { "PINAR": 40.0, "İÇİM": 20.0, "SÜTAŞ": 20.0, "TORKU": 20.0 }
            audit.BrandDistributionJson = products.Count > 0
                ? JsonSerializer.Serialize(
                    products
                        .GroupBy(p => Normalize(p.BrandName))
                        .ToDictionary(
                            g => g.Key,
                            g => Math.Round(g.Count() / (double)products.Count * 100, 1)),
                    JsonOptions)
                : null;

            // ── 3. Compliance Score ───────────────────────────────────────────────
            audit.ComplianceScore = audit.ShelfSharePercentage;

            // ── 4. Status ─────────────────────────────────────────────────────────
            audit.Status = audit.ComplianceScore >= 80 ? AuditStatus.COMPLIANT
                         : audit.ComplianceScore >= 60 ? AuditStatus.WARNING
                                                       : AuditStatus.NON_COMPLIANT;

            await _auditRepository.UpdateAuditAsync(audit);
        }
    }
}