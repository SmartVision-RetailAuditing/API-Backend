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
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetMyTasks([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            int userId = int.Parse(userIdString);

            var tasks = await _taskService.GetTasksByUserIdAsync(userId, page, size);
            return Ok(tasks);
        }

        [HttpGet]
        [Authorize(Roles = "SUPERVISOR,ADMIN")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllTasks([FromQuery] int page = 1, [FromQuery] int size = 20)
        {
            var tasks = await _taskService.GetAllTasksAsync(page, size);
            return Ok(tasks);
        }
        

        // GET: api/tasks/5
        [HttpGet("{id}")]
        [Authorize(Roles = "SUPERVISOR,ADMIN,FIELD_WORKER")]
        public async Task<ActionResult<TaskDto>> GetTaskById(int id)
        {
            // 1. Görevi veritabanýndan getir
            var task = await _taskService.GetTaskByIdAsync(id);

            if (task == null)
            {
                return NotFound(new { message = "Task not found." });
            }

            // 2. GÜVENLÝK KONTROLÜ (Security Check)
            // Þu an sisteme giriþ yapmýþ kullanýcýnýn Rolünü ve ID'sini alýyoruz
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Eðer kullanýcý SAHA ELEMANI ise ve ID'si parse edilebiliyorsa kontrol et
            if (userRole == "FIELD_WORKER" && !string.IsNullOrEmpty(userIdString))
            {
                int currentUserId = int.Parse(userIdString);

                // Görevin sahibi (AssigneeId), þu anki kullanýcý (currentUserId) deðilse?
                if (task.AssigneeId != currentUserId)
                {
                    // 403 Forbidden: Yetkin var ama bu kaynaða eriþimin yok.
                    return StatusCode(403, new { message = "You are not authorized to view this task." });
                }
            }

            // Admin veya Supervisor ise, ya da görev kendisine aitse buraya düþer
            return Ok(task);
        }


        // POST: api/tasks (Create)
        [HttpPost]
        [Authorize(Roles = "SUPERVISOR,ADMIN")]
        public async Task<ActionResult<TaskDto>> CreateTask(CreateTaskDto request)
        {
            var createdTask = await _taskService.CreateTaskAsync(request);
            // For convenience, CreatedAtAction has been changed to OK. We'll look into it later.
            return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "SUPERVISOR,ADMIN,FIELD_WORKER")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto request)
        {
            // 1. Controller sadece adamýn KÝM olduðunu bulur
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            int currentUserId = string.IsNullOrEmpty(userIdString) ? 0 : int.Parse(userIdString);

            try
            {
                // 2. Ýþi Service'e devreder (currentUserId ve userRole'ü de parametre olarak yollarýz)
                var result = await _taskService.UpdateTaskAsync(id, request, currentUserId, userRole);
                if (!result) return NotFound(new { message = "Task not found." });

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                // 3. Service "Bu adamýn yetkisi yok" diye hata fýrlatýrsa 403 döner
                return StatusCode(403, new { message = ex.Message });
            }
        }


        // DELETE: api/tasks/5 (Delete)
        [HttpDelete("{id}")]
        [Authorize(Roles = "SUPERVISOR,ADMIN")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var result = await _taskService.DeleteTaskAsync(id);
            if (!result) return NotFound(new { message = "Task not found." });
            return NoContent();
        }
    }
}
