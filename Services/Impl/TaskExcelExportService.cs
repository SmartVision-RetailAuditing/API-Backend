using ApiBackend.Data;
using ApiBackend.Entities;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Services.Impl
{
    public class TaskExcelExportService
    {
        private readonly AppDbContext _context;

        public TaskExcelExportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateAsync(
            string? search = null,
            string? status = null,
            string? priority = null,
            string? taskType = null,
            int? userId = null)
        {
            var query = _context.Tasks
                .Include(t => t.Store)
                .Include(t => t.User)
                .Include(t => t.Audit)  // ← Audit navigation property ekle
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(t =>
                    t.Store.Name.ToLower().Contains(lower) ||
                    (t.User != null && t.User.FullName.ToLower().Contains(lower)));
            }

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<AuditTaskStatus>(status.ToUpper(), out var parsedStatus))
                query = query.Where(t => t.Status == parsedStatus);

            if (!string.IsNullOrWhiteSpace(priority) &&
                Enum.TryParse<TaskPriority>(priority.ToUpper(), out var parsedPriority))
                query = query.Where(t => t.Priority == parsedPriority);

            if (!string.IsNullOrWhiteSpace(taskType) &&
                Enum.TryParse<TaskType>(taskType.ToUpper(), out var parsedType))
                query = query.Where(t => t.TaskType == parsedType);

            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);

            var tasks = await query.OrderByDescending(t => t.DueDate).ToListAsync();

            // ═══ UTC → Türkiye saati ═══
            var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);
            // ═══════════════════════════

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Tasks");

            var headers = new[]
            {
                "Task ID", "Store", "Address", "Task Type", "Assignee",
                "Priority", "Status", "Due Date", "Completed At", "Description", "Audit ID"
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

            for (int r = 0; r < tasks.Count; r++)
            {
                var t = tasks[r];
                var row = r + 2;
                var bg = r % 2 == 1 ? "#F8FAFC" : "#FFFFFF";

                // ═══ Due Date ve Completed At'i local time'a çevir ═══
                var dueDateLocal = TimeZoneInfo.ConvertTimeFromUtc(t.DueDate, turkeyTimeZone);
                var completedAtLocal = t.CompletedAt.HasValue
                    ? TimeZoneInfo.ConvertTimeFromUtc(t.CompletedAt.Value, turkeyTimeZone)
                    : (DateTime?)null;
                // ═══════════════════════════════════════════════════════

                ws.Cell(row, 1).Value = t.Id;
                ws.Cell(row, 2).Value = t.Store?.Name ?? "";
                ws.Cell(row, 3).Value = t.Store?.Address ?? "";
                ws.Cell(row, 4).Value = t.TaskType.ToString().Replace("_", " ");
                ws.Cell(row, 5).Value = t.User?.FullName ?? "Unassigned";
                ws.Cell(row, 6).Value = t.Priority.ToString();
                ws.Cell(row, 7).Value = t.Status.ToString().Replace("_", " ");
                ws.Cell(row, 8).Value = dueDateLocal;  // ← local time
                ws.Cell(row, 8).Style.NumberFormat.Format = "dd/mm/yyyy";
                ws.Cell(row, 9).Value = completedAtLocal.HasValue
                    ? completedAtLocal.Value.ToString("dd/MM/yyyy HH:mm")
                    : "—";
                ws.Cell(row, 10).Value = t.Description ?? "";
                ws.Cell(row, 11).Value = t.Audit != null ? t.Audit.Id : "—";

                // Priority rengi
                ws.Cell(row, 6).Style.Fill.BackgroundColor = t.Priority switch
                {
                    TaskPriority.HIGH => XLColor.FromHtml("#FEE2E2"),
                    TaskPriority.MEDIUM => XLColor.FromHtml("#FEF3C7"),
                    _ => XLColor.FromHtml("#D1FAE5")
                };

                // Status rengi
                ws.Cell(row, 7).Style.Fill.BackgroundColor = t.Status switch
                {
                    AuditTaskStatus.COMPLETED => XLColor.FromHtml("#D1FAE5"),
                    AuditTaskStatus.IN_PROGRESS => XLColor.FromHtml("#DBEAFE"),
                    _ => XLColor.FromHtml("#FEF3C7")
                };

                // Overdue — due date kırmızı (local time ile karşılaştır)
                if (dueDateLocal < now && t.Status != AuditTaskStatus.COMPLETED)
                    ws.Cell(row, 8).Style.Font.FontColor = XLColor.FromHtml("#EF4444");

                // Zebra
                for (int c = 1; c <= headers.Length; c++)
                {
                    if (c == 6 || c == 7) continue;
                    var cell = ws.Cell(row, c);
                    if (cell.Style.Fill.BackgroundColor == XLColor.NoColor ||
                        cell.Style.Fill.BackgroundColor == XLColor.White)
                        cell.Style.Fill.BackgroundColor = XLColor.FromHtml(bg);
                }
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}