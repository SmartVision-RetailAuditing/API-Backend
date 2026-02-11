using ApiBackend.DTOs.TaskDtos;

namespace ApiBackend.Services.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(int userId);
        Task<IEnumerable<TaskDto>> GetAllTasksAsync();
        Task<TaskDto> CreateTaskAsync(CreateTaskDto request);
        Task<bool> UpdateTaskAsync(int id, CreateTaskDto taskDto);
        Task<bool> DeleteTaskAsync(int id);
    }
}
