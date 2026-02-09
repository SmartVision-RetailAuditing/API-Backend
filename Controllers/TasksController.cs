using ApiBackend.Data;
using ApiBackend.DTOs.TaskDtos;
using ApiBackend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ApiBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bu controller'a sadece giriþ yapanlar eriþebilir!
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        // 1. MOBÝL ÝÇÝN: "Benim Görevlerim"
        // GET: api/tasks/my-tasks
        [HttpGet("my-tasks")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetMyTasks()
        {
            // Token'dan User ID'yi çekme
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            int userId = int.Parse(userIdString);

            var tasks = await _context.Tasks
                .Include(t => t.Store) // Maðaza bilgilerini de getir (Join)
                .Include(t => t.User)  // Kullanýcý bilgisini de getir
                .Where(t => t.UserId == userId) // Sadece BU kullanýcýnýn görevleri
                .OrderBy(t => t.DueDate) // Tarihe göre sýrala
                .Select(t => new TaskDto
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
                    Description = t.Description,
                    AssigneeId = t.UserId,
                    AssigneeName = t.User.FullName
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // 2. WEB PANEL ÝÇÝN: "Tüm Görevler"
        // GET: api/tasks
        // Sadece Admin ve Supervisor görebilsin
        // [Authorize(Roles = "ADMIN,SUPERVISOR")] // Rol kontrolünü sonra açabiliriz
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllTasks()
        {
            var tasks = await _context.Tasks
                .Include(t => t.Store)
                .Include(t => t.User)
                .OrderByDescending(t => t.DueDate)
                .Select(t => new TaskDto
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
                    Description = t.Description,
                    AssigneeId = t.UserId,
                    AssigneeName = t.User.FullName
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // 3. WEB PANEL ÝÇÝN: "Yeni Görev Ata"
        // POST: api/tasks
        [HttpPost]
        public async Task<ActionResult<TaskDto>> CreateTask(CreateTaskDto request)
        {
            var newTask = new AuditTask
            {
                StoreId = request.StoreId,
                UserId = request.UserId,
                TaskType = request.TaskType,
                Priority = request.Priority,
                DueDate = request.DueDate,
                Description = request.Description,
                Status = AuditTaskStatus.PENDING
            };

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyTasks), new { id = newTask.Id }, request);
        }
    }
}
