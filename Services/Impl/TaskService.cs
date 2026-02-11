using ApiBackend.DTOs;
using ApiBackend.DTOs.TaskDtos;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    /// <summary>
    /// Service layer for task-related business logic and operations
    /// Görev ile ilgili iş mantığı ve işlemleri için servis katmanı
    /// </summary>
    public class TaskService : ITaskService
    {
        // Task repository for database access
        // Veritabanı erişimi için görev repository'si
        private readonly ITaskRepository _taskRepository;

        /// <summary>
        /// Constructor with dependency injection for task repository
        /// Görev repository'si için dependency injection ile constructor
        /// </summary>
        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        /// <summary>
        /// Retrieves all tasks assigned to a specific user
        /// Belirli bir kullanıcıya atanan tüm görevleri getirir
        /// </summary>
        /// <param name="userId">User ID / Kullanıcı ID'si</param>
        /// <returns>List of task DTOs for the user / Kullanıcı için görev DTO'larının listesi</returns>
        public async Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(int userId)
        {
            // Fetch tasks from repository for specific user
            // Belirli kullanıcı için repository'den görevleri çek
            var tasks = await _taskRepository.GetTasksByUserIdAsync(userId);

            // Mapping (Entity -> DTO) is done in service layer
            // Mapping (Entity -> DTO) servis katmanında yapılır
            return tasks.Select(MapToDto);
        }

        /// <summary>
        /// Retrieves all tasks in the system
        /// Sistemdeki tüm görevleri getirir
        /// </summary>
        /// <returns>List of all task DTOs / Tüm görev DTO'larının listesi</returns>
        public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
        {
            // Fetch all tasks from repository
            // Repository'den tüm görevleri çek
            var tasks = await _taskRepository.GetAllTasksAsync();

            // Map all tasks to DTOs
            // Tüm görevleri DTO'lara dönüştür
            return tasks.Select(MapToDto);
        }

        /// <summary>
        /// Creates a new task in the system
        /// Sistemde yeni bir görev oluşturur
        /// </summary>
        /// <param name="request">Task creation request DTO / Görev oluşturma istek DTO'su</param>
        /// <returns>Created task DTO with full details / Tam detaylarıyla oluşturulan görev DTO'su</returns>
        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto request)
        {
            // Step 1: Create new task entity
            // Adım 1: Yeni görev entity'si oluştur
            var newTask = new AuditTask
            {
                StoreId = request.StoreId,
                UserId = request.UserId,
                TaskType = request.TaskType,
                Priority = request.Priority,
                DueDate = request.DueDate,
                Description = request.Description,
                Status = AuditTaskStatus.PENDING, // Default status for new tasks / Yeni görevler için varsayılan durum
                // CreatedAt can be added here if property exists
                // CreatedAt property varsa buraya eklenebilir
            };

            // Step 2: Save to database
            // Adım 2: Veritabanına kaydet
            await _taskRepository.AddTaskAsync(newTask);

            // After saving, fetch the task with ALL details including related entities
            // Kaydettikten sonra, ilişkili entity'ler dahil TÜM detaylarla görevi çek
            // This is necessary because the initial save doesn't include navigation properties (Store, User)
            // Bu gereklidir çünkü ilk kayıt navigation property'leri (Store, User) içermez
            var completeTask = await _taskRepository.GetTaskByIdAsync(newTask.Id);

            // Step 3: Use our MapToDto method to return the complete object with all related data
            // Adım 3: Tüm ilişkili verilerle birlikte tam objeyi döndürmek için MapToDto metodumuzu kullan
            return MapToDto(completeTask);
        }

        /// <summary>
        /// Maps AuditTask entity to TaskDto with all related information
        /// AuditTask entity'sini tüm ilişkili bilgilerle TaskDto'ya dönüştürür
        /// </summary>
        /// <param name="t">AuditTask entity / AuditTask entity'si</param>
        /// <returns>TaskDto with complete information / Tam bilgilerle TaskDto</returns>
        private TaskDto MapToDto(AuditTask t)
        {
            return new TaskDto
            {
                // Task basic information
                // Görev temel bilgileri
                Id = t.Id,
                StoreId = t.StoreId,
                TaskType = t.TaskType.ToString(),
                Priority = t.Priority.ToString(),
                Status = t.Status.ToString(),
                DueDate = t.DueDate,
                Description = t.Description,

                // Store related information (from navigation property)
                // Mağaza ile ilgili bilgiler (navigation property'den)
                StoreName = t.Store.Name,
                StoreAddress = t.Store.Address,
                Latitude = t.Store.Latitude,
                Longitude = t.Store.Longitude,

                // User related information (from navigation property)
                // Kullanıcı ile ilgili bilgiler (navigation property'den)
                AssigneeId = t.UserId,
                AssigneeName = t.User.FullName
            };
        }

        /// <summary>
        /// Updates an existing task's information
        /// Mevcut bir görevin bilgilerini günceller
        /// </summary>
        /// <param name="id">Task ID / Görev ID'si</param>
        /// <param name="taskDto">Updated task data / Güncellenmiş görev verisi</param>
        /// <returns>True if successful, false if task not found / Başarılıysa true, görev bulunamazsa false</returns>
        public async Task<bool> UpdateTaskAsync(int id, CreateTaskDto taskDto)
        {
            // Find task by ID
            // Görevi ID ile bul
            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null) return false;

            // Update task properties
            // Görev özelliklerini güncelle
            task.StoreId = taskDto.StoreId;
            task.UserId = taskDto.UserId;
            task.TaskType = taskDto.TaskType;
            task.Priority = taskDto.Priority;
            task.DueDate = taskDto.DueDate;
            task.Description = taskDto.Description;

            // Save changes to database
            // Değişiklikleri veritabanına kaydet
            await _taskRepository.UpdateTaskAsync(task);
            return true;
        }

        /// <summary>
        /// Deletes a task from the system
        /// Sistemden bir görevi siler
        /// </summary>
        /// <param name="id">Task ID / Görev ID'si</param>
        /// <returns>True if successful, false if task not found / Başarılıysa true, görev bulunamazsa false</returns>
        public async Task<bool> DeleteTaskAsync(int id)
        {
            // Find task by ID
            // Görevi ID ile bul
            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null) return false;

            // Delete task from database
            // Görevi veritabanından sil
            await _taskRepository.DeleteTaskAsync(task);
            return true;
        }
    }
}
