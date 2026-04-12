using ApiBackend.Data;
using ApiBackend.Entities;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Services.Impl
{
    public class AuditExcelExportService
    {
        private readonly AppDbContext _context;

        public AuditExcelExportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateAsync(
            int? storeId = null,
            string? status = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            // ── Veri çek ─────────────────────────────────────────────────
            var query = _context.Audits
                .Include(a => a.Store)
                .Include(a => a.User)
                .Include(a => a.Task)
                .Include(a => a.Products)
                .Include(a => a.Issues)
                .AsQueryable();

            if (storeId.HasValue)
                query = query.Where(a => a.StoreId == storeId.Value);

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<AuditStatus>(status.ToUpper(), out var parsedStatus))
                query = query.Where(a => a.Status == parsedStatus);

            if (dateFrom.HasValue)
                query = query.Where(a => a.CaptureDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(a => a.CaptureDate <= dateTo.Value);

            var audits = await query
                .OrderByDescending(a => a.CaptureDate)
                .ToListAsync();

            // ── Workbook ─────────────────────────────────────────────────
            using var wb = new XLWorkbook();

            // ── Sheet 1: Audits ───────────────────────────────────────────
            var ws1 = wb.Worksheets.Add("Audits");

            // Başlık satırı
            var auditHeaders = new[]
            {
                "Audit ID", "Store", "Auditor", "Task Type",
                "Capture Date", "Compliance Score", "Shelf Share %",
                "Status", "Total Products", "Total Issues",
                "Critical Issues", "High Issues"
            };

            for (int i = 0; i < auditHeaders.Length; i++)
            {
                var cell = ws1.Cell(1, i + 1);
                cell.Value = auditHeaders[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E40AF");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Veri satırları
            for (int r = 0; r < audits.Count; r++)
            {
                var a = audits[r];
                var row = r + 2;
                var critCount = a.Issues.Count(i => i.Severity == IssueSeverity.CRITICAL);
                var highCount = a.Issues.Count(i => i.Severity == IssueSeverity.HIGH);

                ws1.Cell(row, 1).Value = a.Id;
                ws1.Cell(row, 2).Value = a.Store?.Name ?? "";
                ws1.Cell(row, 3).Value = a.User?.FullName ?? "";
                ws1.Cell(row, 4).Value = a.Task?.TaskType.ToString() ?? "";
                ws1.Cell(row, 5).Value = a.CaptureDate;
                ws1.Cell(row, 5).Style.NumberFormat.Format = "dd/mm/yyyy hh:mm";
                ws1.Cell(row, 6).Value = (double)a.ComplianceScore;
                ws1.Cell(row, 7).Value = (double)a.ShelfSharePercentage;
                ws1.Cell(row, 8).Value = a.Status.ToString();
                ws1.Cell(row, 9).Value = a.Products.Count;
                ws1.Cell(row, 10).Value = a.Issues.Count;
                ws1.Cell(row, 11).Value = critCount;
                ws1.Cell(row, 12).Value = highCount;

                // Renk kodlaması — Status sütunu
                var statusCell = ws1.Cell(row, 8);
                statusCell.Style.Fill.BackgroundColor = a.Status switch
                {
                    AuditStatus.COMPLIANT => XLColor.FromHtml("#D1FAE5"),
                    AuditStatus.WARNING => XLColor.FromHtml("#FEF3C7"),
                    AuditStatus.NON_COMPLIANT => XLColor.FromHtml("#FEE2E2"),
                    _ => XLColor.White
                };

                // Compliance score rengi
                var scoreCell = ws1.Cell(row, 6);
                scoreCell.Style.Fill.BackgroundColor = a.ComplianceScore >= 80
                    ? XLColor.FromHtml("#D1FAE5")
                    : a.ComplianceScore >= 60
                        ? XLColor.FromHtml("#FEF3C7")
                        : XLColor.FromHtml("#FEE2E2");

                // Zebra satır
                if (r % 2 == 1)
                {
                    for (int c = 1; c <= auditHeaders.Length; c++)
                    {
                        var cell = ws1.Cell(row, c);
                        if (cell.Style.Fill.BackgroundColor == XLColor.White ||
                            cell.Style.Fill.BackgroundColor == XLColor.NoColor)
                            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");
                    }
                }
            }

            ws1.Columns().AdjustToContents();
            ws1.SheetView.FreezeRows(1);

            // ── Sheet 2: Products ─────────────────────────────────────────
            var ws2 = wb.Worksheets.Add("Products");

            var productHeaders = new[]
            {
                "Audit ID", "Store", "Capture Date",
                "Product Name", "Brand", "Code", "Volume", "Category",
                "Price (TL)", "Confidence %", "Eye Level", "Shelf Position",
                "Manually Edited"
            };

            for (int i = 0; i < productHeaders.Length; i++)
            {
                var cell = ws2.Cell(1, i + 1);
                cell.Value = productHeaders[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#0D9488");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            int pRow = 2;
            foreach (var a in audits)
            {
                foreach (var p in a.Products)
                {
                    ws2.Cell(pRow, 1).Value = a.Id;
                    ws2.Cell(pRow, 2).Value = a.Store?.Name ?? "";
                    ws2.Cell(pRow, 3).Value = a.CaptureDate;
                    ws2.Cell(pRow, 3).Style.NumberFormat.Format = "dd/mm/yyyy";
                    ws2.Cell(pRow, 4).Value = p.ProductName;
                    ws2.Cell(pRow, 5).Value = p.BrandName;
                    ws2.Cell(pRow, 6).Value = p.ProductCode ?? "";
                    ws2.Cell(pRow, 7).Value = p.Volume ?? "";
                    ws2.Cell(pRow, 8).Value = p.Category ?? "";
                    ws2.Cell(pRow, 9).Value = p.Price.HasValue ? (double)p.Price.Value : 0;
                    ws2.Cell(pRow, 10).Value = Math.Round(p.ConfidenceScore * 100, 1);
                    ws2.Cell(pRow, 11).Value = p.IsEyeLevel ? "Yes" : "No";
                    ws2.Cell(pRow, 12).Value = p.ShelfPosition?.ToString() ?? "—";
                    ws2.Cell(pRow, 13).Value = p.IsManuallyEdited ? "Yes" : "No";

                    if (p.IsManuallyEdited)
                        ws2.Cell(pRow, 13).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEF3C7");

                    if ((pRow - 2) % 2 == 1)
                        for (int c = 1; c <= productHeaders.Length; c++)
                            if (ws2.Cell(pRow, c).Style.Fill.BackgroundColor == XLColor.NoColor ||
                                ws2.Cell(pRow, c).Style.Fill.BackgroundColor == XLColor.White)
                                ws2.Cell(pRow, c).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");

                    pRow++;
                }
            }

            ws2.Columns().AdjustToContents();
            ws2.SheetView.FreezeRows(1);

            // ── Byte array döndür ─────────────────────────────────────────
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}