using System.Text.Json;
using ApiBackend.Entities;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    public class ShelfComplianceRuleService : IShelfComplianceRuleService
    {
        private const string OwnBrand = "PINAR";

        public (List<AuditIssue> Issues, decimal Score, decimal ShelfShare, string? BrandDistJson) EvaluateRules(List<AuditProduct> products)
        {
            var issues = new List<AuditIssue>();
            decimal score = 100.0m;

            if (!products.Any()) return (issues, 0m, 0m, null);

            // --- 1. Raf Payı Hesabı ---
            var ownCount = products.Count(p => p.BrandName.Equals(OwnBrand, StringComparison.OrdinalIgnoreCase));
            var shelfShare = Math.Round((decimal)ownCount / products.Count * 100, 1);

            if (shelfShare < 60.0m)
            {
                issues.Add(new AuditIssue { IssueType = IssueType.LOW_SHELF_SHARE, Severity = IssueSeverity.HIGH, Description = $"{OwnBrand} raf payı %{shelfShare:F1} — sözleşme minimumu %60'ın altında." });
                score -= 15.0m;
            }

            // --- 2. Ürün Bazlı Kurallar (Fiyat, Raf Konumu vb.) ---
            bool isMustHaveProductFound = false;

            foreach (var p in products)
            {
                // Zorunlu ürün kontrolü
                if (p.ProductCode == "153107682") isMustHaveProductFound = true;

                // KURAL: FİYAT KONTROLÜ (Nokta atışı tek fiyat beklentisi)
                decimal expectedPrice = 45.90m; // Sözleşmedeki sabit fiyat (Örnek: 45.90 TL)
                
                if (p.ProductCode == "153106322" && p.Price != expectedPrice)
                {
                    // Açıklamayı da çok daha profesyonel hale getirdik
                    var desc = $"{p.ProductName} fiyatı hatalı! Raftaki: {p.Price} TL, Olması Gereken: {expectedPrice} TL.";
                    
                    // AYNI İHLAL DAHA ÖNCE EKLENMEDİYSE EKLE (Mükerrer kaydı önler)
                    if (!issues.Any(i => i.IssueType == IssueType.WRONG_PRICE && i.Description == desc))
                    {
                        issues.Add(new AuditIssue { IssueType = IssueType.WRONG_PRICE, Severity = IssueSeverity.HIGH, Description = desc });
                        score -= 15.0m; // Cezayı sadece 1 kere kes!
                    }
                }

                // KURAL: RAF POZİSYONU KONTROLÜ
                if (p.ProductName.Contains("Tam Yağlı") && !p.IsEyeLevel)
                {
                    var desc = $"{p.ProductName} premium bir ürün ve göz hizasında olmalı.";
                    
                    // AYNI İHLAL DAHA ÖNCE EKLENMEDİYSE EKLE
                    if (!issues.Any(i => i.IssueType == IssueType.WRONG_SHELF_POSITION && i.Description == desc))
                    {
                        issues.Add(new AuditIssue { IssueType = IssueType.WRONG_SHELF_POSITION, Severity = IssueSeverity.MEDIUM, Description = desc });
                        score -= 5.0m;
                    }
                }
            }

            // --- 3. Eksik Ürün ---
            if (!isMustHaveProductFound)
            {
                issues.Add(new AuditIssue { IssueType = IssueType.MISSING_PRODUCT, Severity = IssueSeverity.CRITICAL, Description = "Sözleşme gereği zorunlu olan '153107682' kodlu ürün rafta bulunamadı!" });
                score -= 25.0m;
            }

            score = score < 0 ? 0 : score;

            // --- 4. Marka Dağılım JSON'u ---
            var total = (double)products.Count;
            var distribution = products.GroupBy(p => p.BrandName)
                                       .ToDictionary(g => g.Key, g => Math.Round(g.Count() / total * 100, 1));
            
            return (issues, score, shelfShare, JsonSerializer.Serialize(distribution));
        }
    }
}