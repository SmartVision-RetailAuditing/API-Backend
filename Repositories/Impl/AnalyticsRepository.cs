using ApiBackend.Data;
using ApiBackend.DTOs.AnalyticsDtos;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Repositories.Impl
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly AppDbContext _context;

        public AnalyticsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AnalyticsDto> GetAnalyticsAsync(int days)
        {
            var since = DateTime.UtcNow.AddDays(-days);

            // Seçilen döneme ait auditler
            var audits = await _context.Audits
                .Include(a => a.Issues)
                .Include(a => a.Store)
                .Where(a => a.CaptureDate >= since)
                .ToListAsync();

            // ── KPI ─────────────────────────────────────────────────────────
            var kpi = new AnalyticsKpiDto
            {
                TotalAudits = audits.Count,
                AvgCompliance = audits.Any()
                    ? Math.Round(audits.Average(a => a.ComplianceScore), 1)
                    : 0,
                CompliantStores = audits
                    .Where(a => a.ComplianceScore >= 80)
                    .Select(a => a.StoreId)
                    .Distinct()
                    .Count(),
                TotalIssues = audits.Sum(a => a.Issues.Count),
            };

            // ── Issue Distribution (Pie chart) ───────────────────────────────
            var issueLabelMap = new Dictionary<string, string>
            {
                { "PLANOGRAM_MISMATCH",   "Planogram Mismatch" },
                { "MISSING_PRODUCT",      "Missing Product"    },
                { "WRONG_PRICE",          "Wrong Price"        },
                { "LOW_SHELF_SHARE",      "Low Shelf Share"    },
                { "WRONG_SHELF_POSITION", "Wrong Position"     },
            };

            var issueDist = audits
                .SelectMany(a => a.Issues)
                .GroupBy(i => i.IssueType.ToString())
                .Select(g => new IssueDistDto
                {
                    IssueType = g.Key,
                    Label = issueLabelMap.TryGetValue(g.Key, out var lbl) ? lbl : g.Key,
                    Count = g.Count(),
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // ── Chain Metrics (Bar chart + Tablo) ────────────────────────────
            // Trend: dönemin ilk yarısı ile ikinci yarısının avg compliance farkı
            var midPoint = since.AddDays(days / 2.0);

            var chainMetrics = audits
                .GroupBy(a => a.Store.ChainName)
                .Select(g =>
                {
                    var avg = Math.Round(g.Average(a => a.ComplianceScore), 1);

                    var firstHalf = g.Where(a => a.CaptureDate < midPoint).ToList();
                    var secondHalf = g.Where(a => a.CaptureDate >= midPoint).ToList();

                    decimal trend = 0;
                    if (firstHalf.Any() && secondHalf.Any())
                        trend = Math.Round(
                            secondHalf.Average(a => a.ComplianceScore) -
                            firstHalf.Average(a => a.ComplianceScore), 1);

                    var status = avg >= 90 ? "Excellent"
                               : avg >= 75 ? "Good"
                               : "Needs Attention";

                    return new ChainMetricsDto
                    {
                        ChainName = g.Key,
                        AvgCompliance = avg,
                        TotalAudits = g.Count(),
                        Trend = trend,
                        Status = status,
                    };
                })
                .OrderByDescending(c => c.AvgCompliance)
                .ToList();

            return new AnalyticsDto
            {
                Kpi = kpi,
                IssueDistribution = issueDist,
                ChainMetrics = chainMetrics,
            };
        }
    }
}