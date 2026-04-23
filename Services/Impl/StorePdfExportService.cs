using ApiBackend.Data;
using ApiBackend.Entities;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApiBackend.Services.Impl
{
    public class StorePdfExportService
    {
        private readonly AppDbContext _context;

        public StorePdfExportService(AppDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateAsync(int storeId)
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.Id == storeId)
                ?? throw new KeyNotFoundException($"Store {storeId} bulunamadı.");

            var audits = await _context.Audits
                .Include(a => a.User)
                .Include(a => a.Issues)
                .Include(a => a.Products)
                .Where(a => a.StoreId == storeId)
                .OrderByDescending(a => a.CaptureDate)
                .ToListAsync();

            var totalAudits = audits.Count;
            var avgCompliance = totalAudits > 0 ? Math.Round(audits.Average(a => (double)a.ComplianceScore), 1) : 0.0;
            var compliantCount = audits.Count(a => a.Status == AuditStatus.COMPLIANT);
            var warningCount = audits.Count(a => a.Status == AuditStatus.WARNING);
            var nonComplCount = audits.Count(a => a.Status == AuditStatus.NON_COMPLIANT);
            var totalIssues = audits.Sum(a => a.Issues.Count);
            var criticalIssues = audits.Sum(a => a.Issues.Count(i => i.Severity == IssueSeverity.CRITICAL));

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.8f, Unit.Centimetre);
                    page.DefaultTextStyle(t => t.FontSize(10).FontFamily("Arial"));

                    // ── Header ────────────────────────────────────────────
                    page.Header().Element(header =>
                    {
                        header.Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("SmartVision — Store Report")
                                    .FontSize(18).Bold().FontColor("#1E40AF");
                                col.Item().Text($"{store.Name}  ·  {store.ChainName}")
                                    .FontSize(11).FontColor("#64748B");
                            });
                            row.ConstantItem(130).AlignRight().Column(col =>
                            {
                                col.Item().Text(DateTime.Now.ToString("dd MMM yyyy HH:mm"))  // ← DEĞİŞTİ
                                    .FontSize(9).FontColor("#64748B");
                                col.Item().Text($"Store ID: #{store.Id}")
                                    .FontSize(9).FontColor("#64748B");
                            });
                        });
                    });

                    page.Content().PaddingTop(12).Column(col =>
                    {
                        // ── Store Info ────────────────────────────────────
                        col.Item().Text("Store Information").FontSize(12).Bold().FontColor("#1E40AF");
                        col.Item().PaddingTop(6).Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(5); });
                            void Row(string label, string value)
                            {
                                t.Cell().Background("#F8FAFC").Padding(6)
                                    .Text(label).FontSize(9).FontColor("#64748B").Bold();
                                t.Cell().BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(6)
                                    .Text(value).FontSize(10);
                            }
                            Row("Chain", store.ChainName);
                            Row("Address", store.Address);
                            Row("Region", store.Region ?? "—");
                            Row("Coordinates", $"{store.Latitude}, {store.Longitude}");
                        });

                        col.Item().PaddingTop(16);

                        // ── KPI satırı ────────────────────────────────────
                        col.Item().Text("Compliance Summary").FontSize(12).Bold().FontColor("#1E40AF");
                        col.Item().PaddingTop(6).Row(row =>
                        {
                            void KpiBox(string label, string value, string color)
                            {
                                row.RelativeItem()
                                    .Border(1).BorderColor("#E2E8F0")
                                    .Background("#F8FAFC").Padding(8).Column(c =>
                                    {
                                        c.Item().Text(label).FontSize(8).FontColor("#64748B");
                                        c.Item().Text(value).FontSize(18).Bold().FontColor(color);
                                    });
                            }

                            var scoreColor = avgCompliance >= 80 ? "#10B981"
                                           : avgCompliance >= 60 ? "#F59E0B" : "#EF4444";

                            KpiBox("Avg. Compliance", $"{avgCompliance}%", scoreColor);
                            KpiBox("Total Audits", totalAudits.ToString(), "#1E40AF");
                            KpiBox("Compliant", compliantCount.ToString(), "#10B981");
                            KpiBox("Warning", warningCount.ToString(), "#F59E0B");
                            KpiBox("Non-Compliant", nonComplCount.ToString(), "#EF4444");
                            KpiBox("Total Issues", totalIssues.ToString(), "#64748B");
                            KpiBox("Critical Issues", criticalIssues.ToString(),
                                criticalIssues > 0 ? "#EF4444" : "#64748B");
                        });

                        col.Item().PaddingTop(16);

                        // ── Audit Geçmişi ─────────────────────────────────
                        if (audits.Any())
                        {
                            col.Item().Text("Audit History").FontSize(12).Bold().FontColor("#1E40AF");
                            col.Item().PaddingTop(6).Table(t =>
                            {
                                t.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(30);  // #
                                    c.RelativeColumn(2);   // Date
                                    c.RelativeColumn(2);   // Auditor
                                    c.RelativeColumn(1.5f); // Score
                                    c.RelativeColumn(1.5f); // Status
                                    c.RelativeColumn(1);   // Issues
                                    c.RelativeColumn(1);   // Products
                                });

                                t.Header(h =>
                                {
                                    foreach (var title in new[] { "#", "Date", "Auditor", "Score", "Status", "Issues", "Products" })
                                        h.Cell().Background("#1E40AF").Padding(5)
                                            .Text(title).FontColor("#FFFFFF").Bold().FontSize(9);
                                });

                                foreach (var (a, idx) in audits.Select((a, i) => (a, i + 1)))
                                {
                                    var statusColor = a.Status == AuditStatus.COMPLIANT ? "#10B981"
                                                    : a.Status == AuditStatus.WARNING ? "#F59E0B"
                                                                                              : "#EF4444";
                                    var scoreColor = a.ComplianceScore >= 80 ? "#10B981"
                                                    : a.ComplianceScore >= 60 ? "#F59E0B" : "#EF4444";
                                    var bg = idx % 2 == 0 ? "#F8FAFC" : "#FFFFFF";

                                    t.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(5)
                                        .Text(idx.ToString()).FontSize(9).FontColor("#64748B");
                                    t.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(5)
                                        .Text(a.CaptureDate.ToString("dd MMM yyyy")).FontSize(9);
                                    t.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(5)
                                        .Text(a.User?.FullName ?? "—").FontSize(9);
                                    t.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(5)
                                        .Text($"{a.ComplianceScore}%").FontSize(9).Bold().FontColor(scoreColor);
                                    t.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(5)
                                        .Text(a.Status.ToString()).FontSize(9).FontColor(statusColor);
                                    t.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(5)
                                        .Text(a.Issues.Count.ToString()).FontSize(9);
                                    t.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(5)
                                        .Text(a.Products.Count.ToString()).FontSize(9);
                                }
                            });
                        }
                        else
                        {
                            col.Item().PaddingTop(8).Text("No audits recorded for this store.")
                                .FontSize(10).FontColor("#94A3B8").Italic();
                        }
                    });

                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("SmartVision  ·  Generated ").FontSize(8).FontColor("#94A3B8");
                        t.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm")).FontSize(8).FontColor("#94A3B8");  // ← DEĞİŞTİ
                        t.Span("  ·  Page ").FontSize(8).FontColor("#94A3B8");
                        t.CurrentPageNumber().FontSize(8).FontColor("#94A3B8");
                        t.Span(" of ").FontSize(8).FontColor("#94A3B8");
                        t.TotalPages().FontSize(8).FontColor("#94A3B8");
                    });
                });
            });

            return doc.GeneratePdf();
        }
    }
}