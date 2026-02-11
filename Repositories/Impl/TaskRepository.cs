using ApiBackend.Data;
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

        public async Task<IEnumerable<AuditTask>> GetTasksByUserIdAsync(int userId)
        {
            return await _context.Tasks
                .Include(t => t.Store)
                .Include(t => t.User)
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditTask>> GetAllTasksAsync()
        {
            return await _context.Tasks
                .Include(t => t.Store)
                .Include(t => t.User)
                .OrderByDescending(t => t.DueDate)
                .ToListAsync();
        }

        public async Task AddTaskAsync(AuditTask task)
        {
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
        }

        public async Task<AuditTask?> GetTaskByIdAsync(int id)
        {
            // FindAsync yerine FirstOrDefaultAsync ve Include kullanıyoruz
            // Böylece Task gelirken yanında Mağaza ve Kullanıcı bilgilerini de getiriyor.
            return await _context.Tasks
                .Include(t => t.Store)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);
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
