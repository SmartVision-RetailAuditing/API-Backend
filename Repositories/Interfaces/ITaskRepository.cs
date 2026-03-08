using ApiBackend.Entities;

namespace ApiBackend.Repositories.Interfaces
{
    public interface ITaskRepository
    {
        Task<IEnumerable<AuditTask>> GetTasksByUserIdAsync(int userId);
        Task<IEnumerable<AuditTask>> GetTasksByUserIdAsync(int userId, int pageNumber, int pageSize);
        Task<IEnumerable<AuditTask>> GetAllTasksAsync(int pageNumber, int pageSize);
        Task<AuditTask?> GetTaskByIdAsync(int id); // Tekil görev bulmak için
        Task AddTaskAsync(AuditTask task);
        Task UpdateTaskAsync(AuditTask task);
        Task DeleteTaskAsync(AuditTask task);
    }
}
