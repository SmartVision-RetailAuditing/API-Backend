using ApiBackend.Data;
using ApiBackend.Entities;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApiBackend.Services.Impl
{
    public class AuditPdfExportService
    {
        private readonly AppDbContext _context;

        public AuditPdfExportService(AppDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateAsync(int auditId)
        {
            var audit = await _context.Audits
                .Include(a => a.Store)
                .Include(a => a.User)
                .Include(a => a.Task)
                .Include(a => a.Products)
                .Include(a => a.Issues)
                .FirstOrDefaultAsync(a => a.Id == auditId)
                ?? throw new KeyNotFoundException($"Audit {auditId} bulunamadı.");

            // ═══ UTC → Türkiye saati çevirme ═══
            var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            var auditDateLocal = TimeZoneInfo.ConvertTimeFromUtc(audit.CaptureDate, turkeyTimeZone);
            var generatedDateLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);
            // ═══════════════════════════════════

            // Brand distribution parse
            var brandData = new Dictionary<string, double>();
            if (!string.IsNullOrEmpty(audit.BrandDistributionJson))
            {
                try
                {
                    brandData = System.Text.Json.JsonSerializer
                        .Deserialize<Dictionary<string, double>>(audit.BrandDistributionJson)
                        ?? new();
                }
                catch { }
            }

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
                                col.Item().Text("SmartVision — Audit Report")
                                    .FontSize(18).Bold().FontColor("#1E40AF");
                                col.Item().Text($"Audit #{audit.Id}  ·  {audit.Store?.Name}")
                                    .FontSize(11).FontColor("#64748B");
                            });
                            row.ConstantItem(120).AlignRight().Column(col =>
                            {
                                col.Item().Text(auditDateLocal.ToString("dd MMM yyyy HH:mm"))
                                    .FontSize(9).FontColor("#64748B");
                                col.Item().Text($"Auditor: {audit.User?.FullName ?? "—"}")
                                    .FontSize(9).FontColor("#64748B");
                            });
                        });
                    });

                    page.Content().PaddingTop(12).Column(col =>
                    {
                        // ── KPI Satırı ────────────────────────────────────
                        col.Item().Row(row =>
                        {
                            void KpiBox(string label, string value, string color)
                            {
                                row.RelativeItem().Border(1).BorderColor("#E2E8F0")
                                    .Background("#F8FAFC").Padding(8).Column(c =>
                                    {
                                        c.Item().Text(label).FontSize(8).FontColor("#64748B");
                                        c.Item().Text(value).FontSize(16).Bold().FontColor(color);
                                    });
                            }

                            var scoreColor = audit.ComplianceScore >= 80 ? "#10B981"
                                           : audit.ComplianceScore >= 60 ? "#F59E0B" : "#EF4444";
                            var statusColor = audit.Status == AuditStatus.COMPLIANT ? "#10B981"
                                            : audit.Status == AuditStatus.WARNING ? "#F59E0B" : "#EF4444";

                            KpiBox("Compliance Score", $"{audit.ComplianceScore}%", scoreColor);
                            KpiBox("Shelf Share", $"{audit.ShelfSharePercentage}%", "#0D9488");
                            KpiBox("Status", audit.Status.ToString(), statusColor);
                            KpiBox("Total Products", audit.Products.Count.ToString(), "#1E40AF");
                            KpiBox("Issues Detected", audit.Issues.Count.ToString(),
                                   audit.Issues.Any(i => i.Severity == IssueSeverity.CRITICAL) ? "#EF4444" : "#64748B");
                        });

                        col.Item().PaddingTop(14);

                        // ── Brand Distribution ────────────────────────────
                        if (brandData.Any())
                        {
                            col.Item().Text("Brand Distribution").FontSize(12).Bold().FontColor("#1E40AF");
                            col.Item().PaddingTop(4).Table(t =>
                            {
                                t.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(3);
                                    c.RelativeColumn(1);
                                });
                                t.Header(h =>
                                {
                                    h.Cell().Background("#1E40AF").Padding(5)
                                        .Text("Brand").FontColor("#FFFFFF").Bold().FontSize(9);
                                    h.Cell().Background("#1E40AF").Padding(5)
                                        .Text("Share %").FontColor("#FFFFFF").Bold().FontSize(9);
                                });
                                foreach (var (brand, pct) in brandData.OrderByDescending(x => x.Value))
                                {
                                    t.Cell().BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(5)
                                        .Text(brand).FontSize(9);
                                    t.Cell().BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(5)
                                        .Text($"{pct:F1}%").FontSize(9);
                                }
                            });
                            col.Item().PaddingTop(14);
                        }

                        // ── Issues ────────────────────────────────────────
                        if (audit.Issues.Any())
                        {
                            col.Item().Text("Issues").FontSize(12).Bold().FontColor("#1E40AF");
                            col.Item().PaddingTop(4).Table(t =>
                            {
                                t.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(1);
                                    c.RelativeColumn(4);
                                });
                                t.Header(h =>
                                {
                                    foreach (var title in new[] { "Type", "Severity", "Description" })
                                        h.Cell().Background("#1E40AF").Padding(5)
                                            .Text(title).FontColor("#FFFFFF").Bold().FontSize(9);
                                });
                                foreach (var issue in audit.Issues.OrderByDescending(i => i.Severity))
                                {
                                    var sevColor = issue.Severity == IssueSeverity.CRITICAL ? "#EF4444"
                                                 : issue.Severity == IssueSeverity.HIGH ? "#F97316"
                                                 : issue.Severity == IssueSeverity.MEDIUM ? "#F59E0B"
                                                                                             : "#3B82F6";
                                    t.Cell().BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(5)
                                        .Text(issue.IssueType.ToString().Replace("_", " ")).FontSize(9);
                                    t.Cell().BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(5)
                                        .Text(issue.Severity.ToString()).FontSize(9).FontColor(sevColor).Bold();
                                    t.Cell().BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(5)
                                        .Text(issue.Description).FontSize(9);
                                }
                            });
                            col.Item().PaddingTop(14);
                        }

                        // ── Products ──────────────────────────────────────
                        col.Item().Text("Detected Products").FontSize(12).Bold().FontColor("#1E40AF");
                        col.Item().PaddingTop(4).Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3); // Product Name
                                c.RelativeColumn(2); // Brand
                                c.RelativeColumn(2); // Code
                                c.RelativeColumn(1); // Price
                                c.RelativeColumn(1); // Confidence
                                c.RelativeColumn(1); // Edited
                            });
                            t.Header(h =>
                            {
                                foreach (var title in new[] { "Product", "Brand", "Code", "Price", "Conf.", "Edited" })
                                    h.Cell().Background("#1E40AF").Padding(5)
                                        .Text(title).FontColor("#FFFFFF").Bold().FontSize(9);
                            });
                            foreach (var p in audit.Products)
                            {
                                t.Cell().BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(4)
                                    .Text(p.ProductName).FontSize(8.5f);
                                t.Cell().BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(4)
                                    .Text(p.BrandName).FontSize(8.5f);
                                t.Cell().BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(4)
                                    .Text(p.ProductCode ?? "—").FontSize(8.5f).FontColor("#64748B");
                                t.Cell().BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(4)
                                    .Text(p.Price.HasValue ? $"{p.Price:F2} ₺" : "—").FontSize(8.5f);
                                t.Cell().BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(4)
                                    .Text($"{p.ConfidenceScore * 100:F0}%").FontSize(8.5f);
                                t.Cell().BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(4)
                                    .Text(p.IsManuallyEdited ? "Yes" : "—")
                                    .FontSize(8.5f).FontColor(p.IsManuallyEdited ? "#F59E0B" : "#64748B");
                            }
                        });
                    });

                    // ── Footer ────────────────────────────────────────────
                    page.Footer().AlignCenter()
                        .Text(t =>
                        {
                            t.Span("SmartVision  ·  Generated ").FontSize(8).FontColor("#94A3B8");
                            t.Span(generatedDateLocal.ToString("dd MMM yyyy HH:mm")).FontSize(8).FontColor("#94A3B8");
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