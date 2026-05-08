using ApiBackend.Data;
using ApiBackend.Entities;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApiBackend.Services.Impl
{
    public class TaskPdfExportService
    {
        private readonly AppDbContext _context;

        public TaskPdfExportService(AppDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateAsync(int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.Store)
                .Include(t => t.User)
                .Include(t => t.Audit)
                    .ThenInclude(a => a != null ? a.Issues : null)
                .Include(t => t.Audit)
                    .ThenInclude(a => a != null ? a.Products : null)
                .FirstOrDefaultAsync(t => t.Id == taskId)
                ?? throw new KeyNotFoundException($"Task {taskId} bulunamadı.");

            // ═══ UTC → Türkiye saati ═══
            var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            var generatedDateLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);
            var dueDateLocal = TimeZoneInfo.ConvertTimeFromUtc(task.DueDate, turkeyTimeZone);
            var completedAtLocal = task.CompletedAt.HasValue
                ? TimeZoneInfo.ConvertTimeFromUtc(task.CompletedAt.Value, turkeyTimeZone)
                : (DateTime?)null;
            // ═══════════════════════════

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
                                col.Item().Text("SmartVision — Task Report")
                                    .FontSize(18).Bold().FontColor("#1E40AF");
                                col.Item().Text($"Task #{task.Id}  ·  {task.Store?.Name}")
                                    .FontSize(11).FontColor("#64748B");
                            });
                            row.ConstantItem(130).AlignRight().Column(col =>
                            {
                                // ← local time kullan
                                col.Item().Text(generatedDateLocal.ToString("dd MMM yyyy HH:mm"))
                                    .FontSize(9).FontColor("#64748B");
                                col.Item().Text($"Type: {task.TaskType}")
                                    .FontSize(9).FontColor("#64748B");
                            });
                        });
                    });

                    page.Content().PaddingTop(12).Column(col =>
                    {
                        // ── Status + Priority badges ──────────────────────
                        col.Item().Row(row =>
                        {
                            void Badge(string label, string value, string bg, string fg)
                            {
                                // ← Padding düzeltildi: .Padding(horizontal, vertical) değil .PaddingHorizontal ve .PaddingVertical
                                row.AutoItem().PaddingRight(8).Background(bg)
                                    .PaddingHorizontal(6).PaddingVertical(4)
                                    .Text($"{label}: {value}")
                                    .FontSize(10).Bold().FontColor(fg);
                            }

                            var statusBg = task.Status switch
                            {
                                AuditTaskStatus.COMPLETED => "#D1FAE5",
                                AuditTaskStatus.IN_PROGRESS => "#DBEAFE",
                                _ => "#FEF3C7"
                            };
                            var statusFg = task.Status switch
                            {
                                AuditTaskStatus.COMPLETED => "#065F46",
                                AuditTaskStatus.IN_PROGRESS => "#1E40AF",
                                _ => "#92400E"
                            };
                            var priorityBg = task.Priority switch
                            {
                                TaskPriority.HIGH => "#FEE2E2",
                                TaskPriority.MEDIUM => "#FEF3C7",
                                _ => "#D1FAE5"
                            };
                            var priorityFg = task.Priority switch
                            {
                                TaskPriority.HIGH => "#991B1B",
                                TaskPriority.MEDIUM => "#92400E",
                                _ => "#065F46"
                            };

                            Badge("Status", task.Status.ToString(), statusBg, statusFg);
                            Badge("Priority", task.Priority.ToString(), priorityBg, priorityFg);
                        });

                        col.Item().PaddingTop(14);

                        // ── Task Bilgileri ────────────────────────────────
                        col.Item().Text("Task Information").FontSize(12).Bold().FontColor("#1E40AF");
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
                            Row("Store", task.Store?.Name ?? "—");
                            Row("Address", task.Store?.Address ?? "—");
                            Row("Task Type", task.TaskType.ToString().Replace("_", " "));
                            Row("Assignee", task.User?.FullName ?? "Unassigned");
                            Row("Due Date", dueDateLocal.ToString("dd MMM yyyy"));  // ← local time
                            // ← CreatedAt kaldırıldı (AuditTask'ta yok)
                            if (completedAtLocal.HasValue)
                                Row("Completed", completedAtLocal.Value.ToString("dd MMM yyyy HH:mm"));  // ← local time
                            if (!string.IsNullOrEmpty(task.Description))
                                Row("Description", task.Description);
                        });

                        col.Item().PaddingTop(16);

                        // ── Linked Audit ──────────────────────────────────
                        if (task.Audit != null)
                        {
                            var audit = task.Audit;
                            col.Item().Text("Linked Audit Result").FontSize(12).Bold().FontColor("#1E40AF");
                            col.Item().PaddingTop(6).Row(row =>
                            {
                                void KpiBox(string label, string value, string color)
                                {
                                    row.RelativeItem()
                                        .Border(1).BorderColor("#E2E8F0")
                                        .Background("#F8FAFC").Padding(8).Column(c =>
                                        {
                                            c.Item().Text(label).FontSize(8).FontColor("#64748B");
                                            c.Item().Text(value).FontSize(16).Bold().FontColor(color);
                                        });
                                }

                                var scoreColor = audit.ComplianceScore >= 80 ? "#10B981"
                                               : audit.ComplianceScore >= 60 ? "#F59E0B" : "#EF4444";

                                KpiBox("Audit #", audit.Id.ToString(), "#1E40AF");
                                KpiBox("Compliance", $"{audit.ComplianceScore}%", scoreColor);
                                KpiBox("Shelf Share", $"{audit.ShelfSharePercentage}%", "#0D9488");
                                KpiBox("Status", audit.Status.ToString(), scoreColor);
                                KpiBox("Issues", audit.Issues.Count.ToString(),
                                    audit.Issues.Any(i => i.Severity == IssueSeverity.CRITICAL) ? "#EF4444" : "#64748B");
                                KpiBox("Products", audit.Products?.Count.ToString() ?? "0", "#64748B");
                            });

                            col.Item().PaddingTop(10);

                            // Issues listesi
                            if (audit.Issues.Any())
                            {
                                col.Item().Text("Issues Detected").FontSize(11).Bold().FontColor("#374151");
                                col.Item().PaddingTop(4).Table(t =>
                                {
                                    t.ColumnsDefinition(c =>
                                    {
                                        c.RelativeColumn(2);
                                        c.RelativeColumn(1.5f);
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
                                            .Text(issue.Severity.ToString()).FontSize(9).Bold().FontColor(sevColor);
                                        t.Cell().BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(5)
                                            .Text(issue.Description).FontSize(9);
                                    }
                                });
                            }
                        }
                        else
                        {
                            col.Item().PaddingTop(8)
                                .Text("No audit linked to this task yet.")
                                .FontSize(10).FontColor("#94A3B8").Italic();
                        }
                    });

                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("SmartVision  ·  Generated ").FontSize(8).FontColor("#94A3B8");
                        // ← local time kullan
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