using ApiBackend.DTOs.AuditDtos;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    /// <summary>
    /// Python AI servisi hazır olana kadar kullanılacak mock implementasyon.
    /// Program.cs'de gerçek AIVisionService yerine bu register edilir.
    /// Blob Storage upload'u gerçek çalışır, sadece AI response sahte döner.
    /// </summary>
    public class MockAIVisionService : IAIVisionService
    {
        public Task<AiVisionResultDto> AnalyzeShelfAsync(string imageUrl)
        {
            var result = new AiVisionResultDto
            {
                ComplianceScore = 78.5m,
                ShelfSharePercentage = null, // backend hesaplasın

                Products = new List<AiProductDto>
                {
                    new()
                    {
                        ProductCode     = "153100000",
                        ProductName     = "Süt Tam Yağlı %3.3 1L",
                        BrandName       = "PINAR",
                        Volume          = "1000 ML",
                        Category        = "SÜT",
                        Price           = 45.90m,
                        ConfidenceScore = 0.94,
                        IsEyeLevel      = true,
                        ShelfPosition   = 1,
                        BoundingBox     = new AiBoundingBoxDto { X=120, Y=45, Width=68, Height=210 }
                    },
                    new()
                    {
                        ProductCode     = "153107682",
                        ProductName     = "Süt %2.5 Yağlı 1L",
                        BrandName       = "PINAR",
                        Volume          = "1000 ML",
                        Category        = "SÜT",
                        Price           = 43.50m,
                        ConfidenceScore = 0.89,
                        IsEyeLevel      = false,
                        ShelfPosition   = 3,
                        BoundingBox     = new AiBoundingBoxDto { X=198, Y=245, Width=65, Height=208 }
                    },
                    new()
                    {
                        ProductCode     = "SEK-001",
                        ProductName     = "Sek Tam Yağlı Süt 1L",
                        BrandName       = "SEK",
                        Volume          = "1000 ML",
                        Category        = "SÜT",
                        Price           = 42.00m,
                        ConfidenceScore = 0.91,
                        IsEyeLevel      = true,
                        ShelfPosition   = 1,
                        BoundingBox     = new AiBoundingBoxDto { X=310, Y=48, Width=70, Height=212 }
                    },
                    new()
                    {
                        ProductCode     = "SUTAS-001",
                        ProductName     = "Sütaş Tam Yağlı Süt 1L",
                        BrandName       = "SUTAŞ",
                        Volume          = "1000 ML",
                        Category        = "SÜT",
                        Price           = 48.00m,
                        ConfidenceScore = 0.87,
                        IsEyeLevel      = false,
                        ShelfPosition   = 2,
                        BoundingBox     = new AiBoundingBoxDto { X=420, Y=150, Width=66, Height=209 }
                    },
                },

                Issues = new List<AiIssueDto>
                {
                    new()
                    {
                        IssueType   = "LOW_SHELF_SHARE",
                        Severity    = "HIGH",
                        Description = "PINAR raf payı %50 — sözleşme minimumu %60'ın altında."
                    },
                    new()
                    {
                        IssueType   = "WRONG_SHELF_POSITION",
                        Severity    = "MEDIUM",
                        Description = "Süt %2.5 Yağlı göz hizasının altında, 3. rafta tespit edildi."
                    }
                }
            };

            return Task.FromResult(result);
        }
    }
}