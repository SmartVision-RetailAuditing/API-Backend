using ApiBackend.DTOs.TaskDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Only logged-in users can access this controller!
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet("my-tasks")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetMyTasks()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            int userId = int.Parse(userIdString);

            var tasks = await _taskService.GetTasksByUserIdAsync(userId);
            return Ok(tasks);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllTasks()
        {
            var tasks = await _taskService.GetAllTasksAsync();
            return Ok(tasks);
        }

        // POST: api/tasks (Create)
        [HttpPost]
        [Authorize(Roles = "SUPERVISOR,ADMIN")]
        public async Task<ActionResult<TaskDto>> CreateTask(CreateTaskDto request)
        {
            var createdTask = await _taskService.CreateTaskAsync(request);
            // For convenience, CreatedAtAction has been changed to OK. We'll look into it later.
            return Ok(createdTask);
        }

        // PUT: api/tasks/5 (Update)
        [HttpPut("{id}")]
        [Authorize(Roles = "SUPERVISOR,ADMIN")]
        public async Task<IActionResult> UpdateTask(int id, CreateTaskDto request)
        {
            var result = await _taskService.UpdateTaskAsync(id, request);
            if (!result) return NotFound();
            return NoContent();
        }

        // DELETE: api/tasks/5 (Delete)
        [HttpDelete("{id}")]
        [Authorize(Roles = "SUPERVISOR,ADMIN")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var result = await _taskService.DeleteTaskAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
