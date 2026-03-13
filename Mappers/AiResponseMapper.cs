using System.Text.Json;
using ApiBackend.DTOs.AuditDtos;
using ApiBackend.Entities;

namespace ApiBackend.Mappers
{
    public class AiResponseMapper
    {
        private const string OwnBrand = "PINAR"; // Tek müşteri — sabit brand

        /// <summary>
        /// AI response'unu Audit + Products + Issues entity'lerine dönüştürür.
        /// ShelfSharePercentage ve BrandDistributionJson backend tarafından hesaplanır.
        /// </summary>
        public (Audit audit, List<AuditProduct> products, List<AuditIssue> issues) Map(
            AiVisionResultDto dto,
            int taskId,
            int storeId,
            int userId,
            string imageUrl)
        {
            var products = MapProducts(dto.Products);
            var issues = MapIssues(dto.Issues);

            var shelfShare = ComputeShelfShare(dto, products);
            var brandDistribution = ComputeBrandDistribution(products);
            var status = DetermineStatus(dto.ComplianceScore);

            var audit = new Audit
            {
                TaskId = taskId,
                StoreId = storeId,
                UserId = userId,
                ImageUrl = imageUrl,
                CaptureDate = DateTime.UtcNow,
                ComplianceScore = dto.ComplianceScore,
                ShelfSharePercentage = shelfShare,
                BrandDistributionJson = brandDistribution,
                Status = status,
            };

            return (audit, products, issues);
        }

        // ── Products ─────────────────────────────────────────────────────────

        private static List<AuditProduct> MapProducts(List<AiProductDto> dtos) =>
            dtos.Select(p => new AuditProduct
            {
                ProductName = p.ProductName,
                ProductCode = p.ProductCode,
                BrandName = p.BrandName,
                Volume = p.Volume,
                Category = p.Category,
                Price = p.Price,
                ConfidenceScore = p.ConfidenceScore,
                IsEyeLevel = p.IsEyeLevel,
                ShelfPosition = p.ShelfPosition,
                BoundingBoxX = p.BoundingBox.X,
                BoundingBoxY = p.BoundingBox.Y,
                BoundingBoxWidth = p.BoundingBox.Width,
                BoundingBoxHeight = p.BoundingBox.Height,
                IsManuallyEdited = false,
            }).ToList();

        // ── Issues ───────────────────────────────────────────────────────────

        private static List<AuditIssue> MapIssues(List<AiIssueDto> dtos)
        {
            var issues = new List<AuditIssue>();

            foreach (var dto in dtos)
            {
                if (!Enum.TryParse<IssueType>(dto.IssueType, ignoreCase: true, out var issueType))
                {
                    // Bilinmeyen issue type — loglanabilir, atlanır
                    continue;
                }

                if (!Enum.TryParse<IssueSeverity>(dto.Severity, ignoreCase: true, out var severity))
                {
                    severity = IssueSeverity.MEDIUM; // Bilinmeyen severity → MEDIUM fallback
                }

                issues.Add(new AuditIssue
                {
                    IssueType = issueType,
                    Severity = severity,
                    Description = dto.Description,
                });
            }

            return issues;
        }

        // ── Hesaplamalar ─────────────────────────────────────────────────────

        /// <summary>
        /// AI ShelfSharePercentage dönerse kullan, döndürmezse
        /// products listesinden PINAR ürün adedi / toplam ürün adedi hesapla.
        /// </summary>
        private static decimal ComputeShelfShare(AiVisionResultDto dto, List<AuditProduct> products)
        {
            if (dto.ShelfSharePercentage.HasValue)
                return dto.ShelfSharePercentage.Value;

            if (!products.Any())
                return 0;

            var ownCount = products.Count(p =>
                p.BrandName.Equals(OwnBrand, StringComparison.OrdinalIgnoreCase));
            var totalCount = products.Count;

            return Math.Round((decimal)ownCount / totalCount * 100, 1);
        }

        /// <summary>
        /// Her brand'in ürün adedi yüzdesini hesaplayıp JSON string döner.
        /// Örnek: {"PINAR": 38.5, "SEK": 31.2, "SUTAŞ": 30.3}
        /// </summary>
        private static string? ComputeBrandDistribution(List<AuditProduct> products)
        {
            if (!products.Any())
                return null;

            var total = (double)products.Count;
            var distribution = products
                .GroupBy(p => p.BrandName.ToUpperInvariant())
                .ToDictionary(
                    g => g.Key,
                    g => Math.Round(g.Count() / total * 100, 1));

            return JsonSerializer.Serialize(distribution);
        }

        /// <summary>
        /// compliance_score → AuditStatus
        /// ≥80 COMPLIANT | ≥60 WARNING | <60 NON_COMPLIANT
        /// </summary>
        private static AuditStatus DetermineStatus(decimal score) =>
            score >= 80 ? AuditStatus.COMPLIANT :
            score >= 60 ? AuditStatus.WARNING :
                          AuditStatus.NON_COMPLIANT;
    }
}