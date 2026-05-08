using ApiBackend.DTOs;
using ApiBackend.DTOs.TaskDtos;
using ApiBackend.Entities;

namespace ApiBackend.Repositories.Interfaces
{
    public interface ITaskRepository
    {
        Task<PagedResult<TaskDto>> GetAllTasksAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            string? status = null,
            string? priority = null,
            string? taskType = null,
            int? userId = null          // YENİ — UserDetailPage için
        );

        // Mobil my-tasks endpoint için — PagedResult
        Task<PagedResult<TaskDto>> GetTasksByUserIdAsync(int userId, int pageNumber, int pageSize);

        // Profil istatistikleri için — tüm tasklar, pagination yok
        Task<IEnumerable<AuditTask>> GetTasksByUserIdAsync(int userId);

        Task<AuditTask?> GetTaskByIdAsync(int id);
        Task AddTaskAsync(AuditTask task);
        Task UpdateTaskAsync(AuditTask task);
        Task DeleteTaskAsync(AuditTask task);
        Task<TaskStatsDto> GetTaskStatsAsync();

    }
}
