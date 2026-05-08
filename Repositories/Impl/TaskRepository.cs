using ApiBackend.Data;
using ApiBackend.DTOs;
using ApiBackend.DTOs.TaskDtos;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Repositories.Impl
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<TaskDto>> GetAllTasksAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            string? status = null,
            string? priority = null,
            string? taskType = null,
            int? userId = null)
        {
            var query = _context.Tasks
                .Include(t => t.Store)
                .Include(t => t.User)
                .Include(t => t.Audit)
                .AsQueryable();

            // Search: StoreName veya AssigneeName
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(t =>
                    t.Store.Name.ToLower().Contains(lower) ||
                    (t.User != null && t.User.FullName.ToLower().Contains(lower))
                );
            }

            // Enum.TryParse pattern — ToString() hatası önlenir
            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<AuditTaskStatus>(status.ToUpper(), out var parsedStatus))
                query = query.Where(t => t.Status == parsedStatus);

            if (!string.IsNullOrWhiteSpace(priority) &&
                Enum.TryParse<TaskPriority>(priority.ToUpper(), out var parsedPriority))
                query = query.Where(t => t.Priority == parsedPriority);

            if (!string.IsNullOrWhiteSpace(taskType) &&
                Enum.TryParse<TaskType>(taskType.ToUpper(), out var parsedTaskType))
                query = query.Where(t => t.TaskType == parsedTaskType);

            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);

            return await ProjectToPagedResult(query, pageNumber, pageSize);
        }

        public async Task<PagedResult<TaskDto>> GetTasksByUserIdAsync(
            int userId, int pageNumber, int pageSize)
        {
            var query = _context.Tasks
                .Include(t => t.Store)
                .Include(t => t.User)
                .Include(t => t.Audit)
                .Where(t => t.UserId == userId)
                .AsQueryable();

            return await ProjectToPagedResult(query, pageNumber, pageSize);
        }

        // Ortak projeksiyon + sayfalama
        private static async Task<PagedResult<TaskDto>> ProjectToPagedResult(
            IQueryable<AuditTask> query, int pageNumber, int pageSize)
        {
            var totalCount = await query.CountAsync();

            var data = await query
                .OrderByDescending(t => t.DueDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    StoreId = t.StoreId,
                    StoreName = t.Store.Name,
                    StoreAddress = t.Store.Address,
                    Latitude = t.Store.Latitude,
                    Longitude = t.Store.Longitude,
                    TaskType = t.TaskType.ToString(),
                    Priority = t.Priority.ToString(),
                    Status = t.Status.ToString(),
                    DueDate = t.DueDate,
                    CompletedAt = t.CompletedAt,
                    Description = t.Description,
                    AssigneeId = t.UserId ?? 0,
                    AssigneeName = t.User != null ? t.User.FullName : string.Empty,
                    AuditId = t.Audit != null ? t.Audit.Id : (int?)null
                })
                .ToListAsync();

            return new PagedResult<TaskDto>
            {
                Data = data,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = pageNumber,
                PageSize = pageSize
            };
        }

        // KPI kartları için
        public async Task<TaskStatsDto> GetTaskStatsAsync()
        {
            var now = DateTime.UtcNow;
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek).Date;

            var totalActive = await _context.Tasks.CountAsync(t =>
                t.Status == AuditTaskStatus.PENDING || t.Status == AuditTaskStatus.IN_PROGRESS);

            var pending = await _context.Tasks.CountAsync(t =>
                t.Status == AuditTaskStatus.PENDING);

            var inProgress = await _context.Tasks.CountAsync(t =>
                t.Status == AuditTaskStatus.IN_PROGRESS);

            var completedThisWeek = await _context.Tasks.CountAsync(t =>
                t.Status == AuditTaskStatus.COMPLETED &&
                t.CompletedAt.HasValue &&
                t.CompletedAt.Value >= startOfWeek);

            var unassigned = await _context.Tasks.CountAsync(t =>
                t.UserId == null);

            return new TaskStatsDto
            {
                TotalActive = totalActive,
                Pending = pending,
                InProgress = inProgress,
                CompletedThisWeek = completedThisWeek,
                Unassigned = unassigned
            };
        }

        // Profil istatistikleri için — tüm tasklar, pagination yok
        public async Task<IEnumerable<AuditTask>> GetTasksByUserIdAsync(int userId)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public async Task<AuditTask?> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.Store)
                .Include(t => t.User)
                .Include(t => t.Audit)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddTaskAsync(AuditTask task)
        {
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTaskAsync(AuditTask task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTaskAsync(AuditTask task)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
    }
}