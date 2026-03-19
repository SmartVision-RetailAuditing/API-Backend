using ApiBackend.DTOs.AuditDtos;
using ApiBackend.Entities;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    public class ShelfComplianceService : IShelfComplianceService
    {
        public void EvaluateRules(AiVisionResultDto aiResult)
        {
            aiResult.Issues ??= new List<AiIssueDto>();

            var totalProducts = aiResult.Products?.Count ?? 0;
            if (totalProducts == 0)
            {
                aiResult.ComplianceScore = 0;
                return;
            }

            // --- KURAL 1: MARKA RAF PAYI (Örn: PINAR en az %60 olmalı) ---
            var targetBrand = "PINAR";
            var brandCount = aiResult.Products!.Count(p => p.BrandName.Equals(targetBrand, StringComparison.OrdinalIgnoreCase));
            var brandShare = (decimal)brandCount / totalProducts * 100;
            
            aiResult.ShelfSharePercentage = brandShare;

            if (brandShare < 60.0m)
            {
                aiResult.Issues.Add(new AiIssueDto
                {
                    IssueType = IssueType.LOW_SHELF_SHARE.ToString(),
                    Severity = IssueSeverity.HIGH.ToString(),
                    Description = $"{targetBrand} raf payı %{brandShare:F1} — sözleşme minimumu %60'ın altında."
                });
            }

            // --- ÜRÜN BAZLI KURALLAR ---
            bool isMustHaveProductFound = false;

            foreach (var product in aiResult.Products!)
            {
                // KURAL 2: FİYAT KONTROLÜ
                if (product.ProductCode == "153106322" && (product.Price > 50.0m || product.Price < 30.0m))
                {
                    aiResult.Issues.Add(new AiIssueDto
                    {
                        IssueType = IssueType.WRONG_PRICE.ToString(),
                        Severity = IssueSeverity.HIGH.ToString(),
                        Description = $"{product.BrandName} ({product.ProductCode}) fiyatı {product.Price} TL. Beklenen aralığın (30-50 TL) dışında!"
                    });
                }

                // KURAL 3: RAF POZİSYONU (Örn: Tam yağlı sütler göz hizasında olmalı)
                bool isPremiumProduct = product.ProductName?.Contains("Tam Yağlı") == true;
                if (isPremiumProduct && !product.IsEyeLevel)
                {
                    aiResult.Issues.Add(new AiIssueDto
                    {
                        IssueType = IssueType.WRONG_SHELF_POSITION.ToString(),
                        Severity = IssueSeverity.MEDIUM.ToString(),
                        Description = $"{product.ProductName} premium bir ürün ve göz hizasında olmalı. Ancak raf {product.ShelfPosition} olarak tespit edildi."
                    });
                }

                // Zorunlu ürün tespiti için kontrol
                if (product.ProductCode == "153107682")
                {
                    isMustHaveProductFound = true;
                }
            }

            // --- KURAL 4: EKSİK ÜRÜN KONTROLÜ (MISSING PRODUCT) ---
            if (!isMustHaveProductFound)
            {
                aiResult.Issues.Add(new AiIssueDto
                {
                    IssueType = IssueType.MISSING_PRODUCT.ToString(),
                    Severity = IssueSeverity.CRITICAL.ToString(),
                    Description = "Sözleşme gereği zorunlu olan '153107682' kodlu ürün rafta bulunamadı!"
                });
            }

            // --- SKOR HESAPLAMA ---
            aiResult.ComplianceScore = CalculateComplianceScore(aiResult.Issues);
        }

        private decimal CalculateComplianceScore(List<AiIssueDto> issues)
        {
            decimal score = 100.0m;

            foreach (var issue in issues)
            {
                if (Enum.TryParse<IssueSeverity>(issue.Severity, out var severity))
                {
                    score -= severity switch
                    {
                        IssueSeverity.CRITICAL => 25.0m,
                        IssueSeverity.HIGH => 15.0m,
                        IssueSeverity.MEDIUM => 5.0m,
                        IssueSeverity.LOW => 2.0m,
                        _ => 0.0m
                    };
                }
            }

            return score < 0 ? 0 : score;
        }
    }
}