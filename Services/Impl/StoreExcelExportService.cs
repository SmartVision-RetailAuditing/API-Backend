using ApiBackend.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Services.Impl
{
    public class StoreExcelExportService
    {
        private readonly AppDbContext _context;

        public StoreExcelExportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateAsync(string? search = null)
        {
            var query = _context.Stores.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(s =>
                    s.Name.ToLower().Contains(lower) ||
                    s.ChainName.ToLower().Contains(lower) ||
                    (s.Region != null && s.Region.ToLower().Contains(lower)));
            }

            // ═══ DEĞİŞİKLİK: Audit verilerini Join ile hesapla ═══
            var storesWithStats = await query
                .GroupJoin(
                    _context.Audits,
                    store => store.Id,
                    audit => audit.StoreId,
                    (store, audits) => new
                    {
                        Store = store,
                        AuditCount = audits.Count(),
                        ComplianceScore = audits.Any()
                            ? Math.Round(audits.Average(a => (double)a.ComplianceScore), 1)
                            : 0.0
                    })
                .Select(x => new
                {
                    x.Store,
                    x.AuditCount,
                    x.ComplianceScore,
                    Status = x.ComplianceScore >= 80 ? "Compliant"
                           : x.ComplianceScore >= 60 ? "Warning"
                           : "Non-Compliant"
                })
                .OrderBy(x => x.Store.ChainName)
                .ThenBy(x => x.Store.Name)
                .ToListAsync();
            // ═══════════════════════════════════════════════════════

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Stores");

            var headers = new[]
            {
                "ID", "Store Name", "Chain", "Region", "Address",
                "Latitude", "Longitude", "Compliance Score", "Status", "Total Audits"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E40AF");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            for (int r = 0; r < storesWithStats.Count; r++)
            {
                var item = storesWithStats[r];
                var s = item.Store;
                var row = r + 2;
                var bg = r % 2 == 1 ? "#F8FAFC" : "#FFFFFF";

                ws.Cell(row, 1).Value = s.Id;
                ws.Cell(row, 2).Value = s.Name;
                ws.Cell(row, 3).Value = s.ChainName;
                ws.Cell(row, 4).Value = s.Region ?? "";
                ws.Cell(row, 5).Value = s.Address;
                ws.Cell(row, 6).Value = s.Latitude;
                ws.Cell(row, 7).Value = s.Longitude;
                ws.Cell(row, 8).Value = item.ComplianceScore;      // ← hesaplanan değer
                ws.Cell(row, 9).Value = item.Status;                // ← hesaplanan değer
                ws.Cell(row, 10).Value = item.AuditCount;           // ← hesaplanan değer

                // Compliance score rengi
                var scoreCell = ws.Cell(row, 8);
                scoreCell.Style.Fill.BackgroundColor = item.ComplianceScore >= 80
                    ? XLColor.FromHtml("#D1FAE5")
                    : item.ComplianceScore >= 60
                        ? XLColor.FromHtml("#FEF3C7")
                        : XLColor.FromHtml("#FEE2E2");

                // Status rengi
                var statusCell = ws.Cell(row, 9);
                statusCell.Style.Fill.BackgroundColor = item.Status switch
                {
                    "Compliant" => XLColor.FromHtml("#D1FAE5"),
                    "Warning" => XLColor.FromHtml("#FEF3C7"),
                    "Non-Compliant" => XLColor.FromHtml("#FEE2E2"),
                    _ => XLColor.White
                };

                // Zebra satır (score ve status hariç)
                for (int c = 1; c <= headers.Length; c++)
                {
                    if (c == 8 || c == 9) continue;
                    var cell = ws.Cell(row, c);
                    if (cell.Style.Fill.BackgroundColor == XLColor.NoColor ||
                        cell.Style.Fill.BackgroundColor == XLColor.White)
                        cell.Style.Fill.BackgroundColor = XLColor.FromHtml(bg);
                }
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            // Özet satırı
            var summaryRow = storesWithStats.Count + 3;
            ws.Cell(summaryRow, 1).Value = "TOTAL";
            ws.Cell(summaryRow, 1).Style.Font.Bold = true;
            ws.Cell(summaryRow, 10).Value = storesWithStats.Sum(x => x.AuditCount);
            ws.Cell(summaryRow, 10).Style.Font.Bold = true;
            ws.Cell(summaryRow, 8).Value = storesWithStats.Any()
                ? Math.Round(storesWithStats.Average(x => x.ComplianceScore), 1)
                : 0;
            ws.Cell(summaryRow, 8).Style.Font.Bold = true;

            for (int c = 1; c <= headers.Length; c++)
                ws.Cell(summaryRow, c).Style.Fill.BackgroundColor = XLColor.FromHtml("#EFF6FF");

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}