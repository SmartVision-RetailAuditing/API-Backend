using ApiBackend.DTOs.TaskDtos;

namespace ApiBackend.Services.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(int userId, int pageNumber, int pageSize);
        Task<IEnumerable<TaskDto>> GetAllTasksAsync(int pageNumber, int pageSize);
        Task<TaskDto> CreateTaskAsync(CreateTaskDto request);
        Task<bool> UpdateTaskAsync(int id, UpdateTaskDto taskDto);
        Task<bool> DeleteTaskAsync(int id);
        Task<TaskDto> GetTaskByIdAsync(int id);
    }
}
