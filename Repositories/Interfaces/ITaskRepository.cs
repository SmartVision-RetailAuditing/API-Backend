using ApiBackend.Entities;

namespace ApiBackend.Repositories.Interfaces
{
    public interface ITaskRepository
    {
        Task<IEnumerable<AuditTask>> GetTasksByUserIdAsync(int userId);
        Task<IEnumerable<AuditTask>> GetAllTasksAsync();
        Task AddTaskAsync(AuditTask task);
        Task<AuditTask?> GetTaskByIdAsync(int id); // Tekil görev bulmak için
        Task UpdateTaskAsync(AuditTask task);
        Task DeleteTaskAsync(AuditTask task);
    }
}
