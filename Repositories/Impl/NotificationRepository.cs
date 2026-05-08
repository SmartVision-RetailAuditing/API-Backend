using ApiBackend.Data;
using ApiBackend.DTOs;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Repositories.Impl
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationDto>> GetNotificationsForRoleAsync(string role, int userId)
        {
            var since = DateTime.UtcNow.AddDays(-7);
            var notifications = new List<NotificationDto>();

            // ── SUPERVISOR + ADMIN: Yeni audit ────────────────────────────
            if (role is "SUPERVISOR" or "ADMIN")
            {
                var newAudits = await _context.Audits
                    .Include(a => a.Store)
                    .Where(a => a.CaptureDate >= since)
                    .OrderByDescending(a => a.CaptureDate)
                    .Take(5)
                    .ToListAsync();

                foreach (var audit in newAudits)
                    notifications.Add(new NotificationDto
                    {
                        Id = $"NEW_AUDIT-{audit.Id}",
                        Type = "NEW_AUDIT",
                        Title = "New Audit Submitted",
                        Message = $"{audit.Store.Name} — Score: {audit.ComplianceScore}%",
                        CreatedAt = audit.CaptureDate,
                        NavigateTo = $"/audits/{audit.Id}",
                    });
            }

            // ── SUPERVISOR + ADMIN: Kritik issue ─────────────────────────
            if (role is "SUPERVISOR" or "ADMIN")
            {
                var criticalIssues = await _context.AuditIssues
                    .Include(i => i.Audit).ThenInclude(a => a.Store)
                    .Where(i => i.Severity == IssueSeverity.CRITICAL
                             && i.Audit.CaptureDate >= since)
                    .OrderByDescending(i => i.Audit.CaptureDate)
                    .Take(5)
                    .ToListAsync();

                foreach (var issue in criticalIssues)
                    notifications.Add(new NotificationDto
                    {
                        Id = $"CRITICAL_ISSUE-{issue.Id}",
                        Type = "CRITICAL_ISSUE",
                        Title = "Critical Issue Detected",
                        Message = $"{issue.Audit.Store.Name} — {issue.IssueType.ToString().Replace('_', ' ')}",
                        CreatedAt = issue.Audit.CaptureDate,
                        NavigateTo = $"/audits/{issue.AuditId}",
                    });
            }

            // ── SUPERVISOR: Görev atandı ──────────────────────────────────
            if (role == "SUPERVISOR")
            {
                var assignedTasks = await _context.Tasks
                    .Include(t => t.Store)
                    .Include(t => t.User)
                    .Where(t => t.UserId != null && t.DueDate >= since)
                    .OrderByDescending(t => t.DueDate)
                    .Take(5)
                    .ToListAsync();

                foreach (var task in assignedTasks)
                    notifications.Add(new NotificationDto
                    {
                        Id = $"TASK_ASSIGNED-{task.Id}",
                        Type = "TASK_ASSIGNED",
                        Title = "Task Assigned",
                        Message = $"{task.User!.FullName} → {task.Store.Name}",
                        CreatedAt = task.DueDate,
                        NavigateTo = $"/tasks/{task.Id}",
                    });
            }

            // ── SUPERVISOR + ADMIN: Overdue tasks ────────────────────────
            if (role is "SUPERVISOR" or "ADMIN")
            {
                var overdueTasks = await _context.Tasks
                    .Include(t => t.Store)
                    .Where(t => t.DueDate < DateTime.UtcNow
                             && t.Status != AuditTaskStatus.COMPLETED)
                    .OrderByDescending(t => t.DueDate)
                    .Take(5)
                    .ToListAsync();

                foreach (var task in overdueTasks)
                    notifications.Add(new NotificationDto
                    {
                        Id = $"OVERDUE_TASK-{task.Id}",
                        Type = "OVERDUE_TASK",
                        Title = "Overdue Task",
                        Message = $"{task.Store.Name} — Due: {task.DueDate:dd MMM yyyy}",
                        CreatedAt = task.DueDate,
                        NavigateTo = $"/tasks/{task.Id}",
                    });
            }

            // ── ADMIN: Yeni kullanıcı ─────────────────────────────────────
            if (role == "ADMIN")
            {
                var newUsers = await _context.Users
                    .Where(u => u.CreatedAt >= since)
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                foreach (var user in newUsers)
                    notifications.Add(new NotificationDto
                    {
                        Id = $"NEW_USER-{user.Id}",
                        Type = "NEW_USER",
                        Title = "New User Added",
                        Message = $"{user.FullName} — {user.Role}",
                        CreatedAt = user.CreatedAt,
                        NavigateTo = $"/users/{user.Id}",
                    });
            }

            return notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .ToList();
        }
    }
}