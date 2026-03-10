using ApiBackend.DTOs;
using ApiBackend.DTOs.TaskDtos;

namespace ApiBackend.Services.Interfaces
{
    public interface ITaskService
    {
        Task<PagedResult<TaskDto>> GetAllTasksAsync(
            int pageNumber, 
            int pageSize,
            string? search = null, 
            string? status = null,
            string? priority = null, 
            string? taskType = null,
            int? userId = null
        );

        Task<TaskStatsDto> GetTaskStatsAsync();

        // Mobil için
        Task<PagedResult<TaskDto>> GetTasksByUserIdAsync(int userId, int pageNumber, int pageSize);

        Task<TaskDto?> GetTaskByIdAsync(int id);
        Task<TaskDto> CreateTaskAsync(CreateTaskDto request);
        Task<bool> UpdateTaskAsync(int id, UpdateTaskDto taskDto, int currentUserId, string userRole);
        Task<bool> DeleteTaskAsync(int id);
    }
}
