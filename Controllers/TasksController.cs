using ApiBackend.DTOs;
using ApiBackend.DTOs.TaskDtos;
using ApiBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ApiBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IEventPublisher _eventPublisher;

        public TasksController(ITaskService taskService, IEventPublisher eventPublisher)
        {
            _taskService = taskService;
            _eventPublisher = eventPublisher;
        }

        // GET: api/tasks/stats
        [HttpGet("stats")]
        public async Task<ActionResult<TaskStatsDto>> GetTaskStats()
        {
            var stats = await _taskService.GetTaskStatsAsync();
            return Ok(stats);
        }

        // GET: api/tasks/my-tasks
        [HttpGet("my-tasks")]
        public async Task<ActionResult<PagedResult<TaskDto>>> GetMyTasks(
            [FromQuery, Range(1, int.MaxValue)] int page = 1,
            [FromQuery, Range(1, 100)] int size = 10)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int userId = int.Parse(userIdStr);
            var result = await _taskService.GetTasksByUserIdAsync(userId, page, size);
            return Ok(result);
        }

        // GET: api/tasks?page=1&size=10&search=migros&status=PENDING&priority=HIGH&taskType=SHELF_AUDIT&userId=5
        [HttpGet]
        [Authorize(Roles = "SUPERVISOR,ADMIN")]
        public async Task<ActionResult<PagedResult<TaskDto>>> GetAllTasks(
            [FromQuery, Range(1, int.MaxValue)] int page = 1,
            [FromQuery, Range(1, 100)] int size = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] string? priority = null,
            [FromQuery] string? taskType = null,
            [FromQuery] int? userId = null)         // YEN� � UserDetailPage i�in
        {
            var result = await _taskService.GetAllTasksAsync(
                page, size, search, status, priority, taskType, userId);
            return Ok(result);
        }

        // GET: api/tasks/5
        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN,SUPERVISOR,FIELD_WORKER")]
        public async Task<ActionResult<TaskDto>> GetTaskById([Range(1, int.MaxValue)] int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound(new { message = "Task not found." });

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userRole == "FIELD_WORKER" && int.TryParse(userIdStr, out var currentUserId))
            {
                if (task.AssigneeId != currentUserId)
                    return StatusCode(403, new { message = "You are not authorized to view this task." });
            }

            return Ok(task);
        }

        // POST: api/tasks
        [HttpPost]
        [Authorize(Roles = "SUPERVISOR,ADMIN")]
        public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto request)
        {
            var created = await _taskService.CreateTaskAsync(request);
            await _eventPublisher.PublishTaskCreatedAsync(created.Id);
            return CreatedAtAction(nameof(GetTaskById), new { id = created.Id }, created);
        }

        // PUT: api/tasks/5
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,SUPERVISOR,FIELD_WORKER")]
        public async Task<IActionResult> UpdateTask(
                [Range(1, int.MaxValue)] int id,
                [FromBody] UpdateTaskDto request)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int currentUserId = int.TryParse(userIdStr, out var uid) ? uid : 0;

            try
            {
                var result = await _taskService.UpdateTaskAsync(id, request, currentUserId, userRole);
                if (!result) return NotFound(new { message = "Task not found." });
                
                await _eventPublisher.PublishTaskUpdatedAsync(id);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        // DELETE: api/tasks/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "SUPERVISOR,ADMIN")]
        public async Task<IActionResult> DeleteTask([Range(1, int.MaxValue)] int id)
        {
            var result = await _taskService.DeleteTaskAsync(id);
            if (!result) return NotFound(new { message = "Task not found." });
            await _eventPublisher.PublishTaskDeletedAsync(id);
            return NoContent();
        }
    }
}
