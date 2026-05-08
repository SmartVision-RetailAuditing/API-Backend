using ApiBackend.DTOs;
using ApiBackend.DTOs.TaskDtos;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;

        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<TaskStatsDto> GetTaskStatsAsync()
        {
            return await _taskRepository.GetTaskStatsAsync();
        }

        public async Task<PagedResult<TaskDto>> GetAllTasksAsync(
            int pageNumber, int pageSize,
            string? search = null, string? status = null,
            string? priority = null, string? taskType = null,
            int? userId = null)
        {
            return await _taskRepository.GetAllTasksAsync(
                pageNumber, pageSize, search, status, priority, taskType, userId);
        }

        public async Task<PagedResult<TaskDto>> GetTasksByUserIdAsync(
            int userId, int pageNumber, int pageSize)
        {
            return await _taskRepository.GetTasksByUserIdAsync(userId, pageNumber, pageSize);
        }

        public async Task<TaskDto?> GetTaskByIdAsync(int id)
        {
            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null) return null;
            return MapToDto(task);
        }

        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto request)
        {
            var newTask = new AuditTask
            {
                StoreId = request.StoreId,
                UserId = request.UserId,
                TaskType = request.TaskType,
                Priority = request.Priority,
                DueDate = request.DueDate,
                Description = request.Description,
                Status = AuditTaskStatus.PENDING,
            };

            await _taskRepository.AddTaskAsync(newTask);
            var complete = await _taskRepository.GetTaskByIdAsync(newTask.Id);
            return MapToDto(complete!);
        }

        public async Task<bool> UpdateTaskAsync(
            int id, UpdateTaskDto taskDto, int currentUserId, string userRole)
        {
            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null) return false;

            if (userRole == "FIELD_WORKER")
            {
                if (task.UserId != currentUserId)
                    throw new UnauthorizedAccessException("You can only update your own tasks.");

                taskDto.StoreId = null;
                taskDto.UserId = null;
                taskDto.TaskType = null;
                taskDto.Priority = null;
                taskDto.DueDate = null;
            }

            var oldStatus = task.Status;

            if (taskDto.StoreId.HasValue) task.StoreId = taskDto.StoreId.Value;
            if (taskDto.UserId.HasValue) task.UserId = taskDto.UserId.Value;
            if (taskDto.TaskType.HasValue) task.TaskType = taskDto.TaskType.Value;
            if (taskDto.Priority.HasValue) task.Priority = taskDto.Priority.Value;
            if (taskDto.DueDate.HasValue) task.DueDate = taskDto.DueDate.Value;
            if (taskDto.Description != null) task.Description = taskDto.Description;

            if (taskDto.Status.HasValue && oldStatus != taskDto.Status.Value)
            {
                task.Status = taskDto.Status.Value;
                task.CompletedAt = task.Status == AuditTaskStatus.COMPLETED
                    ? DateTime.UtcNow : null;
            }

            await _taskRepository.UpdateTaskAsync(task);
            return true;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null) return false;
            await _taskRepository.DeleteTaskAsync(task);
            return true;
        }

        private static TaskDto MapToDto(AuditTask t) => new()
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
            AssigneeName = t.User?.FullName ?? string.Empty,
            AuditId = t.Audit?.Id
        };
    }
}