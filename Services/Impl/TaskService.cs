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
        /// Retrieves all tasks assigned to a specific user (with pagination)
        /// Belirli bir kullanıcıya atanan tüm görevleri getirir (sayfalama ile)
        /// </summary>
        /// <param name="userId">User ID / Kullanıcı ID'si</param>
        /// <param name="page">Page number for pagination / Sayfalama için sayfa numarası</param>
        /// <param name="size">Number of items per page / Sayfa başına öğe sayısı</param>
        /// <returns>List of task DTOs for the user / Kullanıcı için görev DTO'larının listesi</returns>
        public async Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(int userId, int page = 1, int size = 10)
        {
            // Fetch tasks from repository for specific user with pagination
            // Belirli kullanıcı için repository'den görevleri sayfalama ile çek
            var tasks = await _taskRepository.GetTasksByUserIdAsync(userId, page, size);

            // Mapping (Entity -> DTO) is done in service layer
            // Mapping (Entity -> DTO) servis katmanında yapılır
            return tasks.Select(MapToDto);
        }

        /// <summary>
        /// Retrieves all tasks in the system (with pagination)
        /// Sistemdeki tüm görevleri getirir (sayfalama ile)
        /// </summary>
        /// <param name="page">Page number for pagination / Sayfalama için sayfa numarası</param>
        /// <param name="size">Number of items per page / Sayfa başına öğe sayısı</param>
        /// <returns>List of all task DTOs / Tüm görev DTO'larının listesi</returns>
        public async Task<IEnumerable<TaskDto>> GetAllTasksAsync(int page = 1, int size = 10)
        {
            // Fetch all tasks from repository with pagination
            // Repository'den tüm görevleri sayfalama ile çek
            var tasks = await _taskRepository.GetAllTasksAsync(page, size);

            // Map all tasks to DTOs
            // Tüm görevleri DTO'lara dönüştür
            return tasks.Select(MapToDto);
        }

        /// <summary>
        /// Retrieves a specific task by ID
        /// Belirli bir görevi ID ile getirir
        /// </summary>
        /// <param name="id">Task ID / Görev ID'si</param>
        /// <returns>Task DTO or null if not found / Görev DTO'su veya bulunamazsa null</returns>
        public async Task<TaskDto> GetTaskByIdAsync(int id)
        {
            // Fetch task by ID
            // Görevi ID ile çek
            var task = await _taskRepository.GetTaskByIdAsync(id);

            if (task == null) return null;

            // Map entity to DTO
            // Entity'yi DTO'ya dönüştür
            return MapToDto(task);
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

        //// <summary>
        /// Updates an existing task's information
        /// Mevcut bir görevin bilgilerini günceller
        /// </summary>
        /// <param name="id">Task ID / Görev ID'si</param>
        /// <param name="taskDto">Updated task data / Güncellenmiş görev verisi</param>
        /// <returns>True if successful, false if task not found / Başarılıysa true, görev bulunamazsa false</returns>
        public async Task<bool> UpdateTaskAsync(int id, UpdateTaskDto taskDto)
        {
            // Find task by ID
            // Görevi ID ile bul
            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null) return false;

            // First, save the old status.
            // Önce eski status'u sakla
            var oldStatus = task.Status;

            // Update other fields
            // Diğer alanları güncelle
            task.StoreId = taskDto.StoreId;
            task.UserId = taskDto.UserId;
            task.TaskType = taskDto.TaskType;
            task.Priority = taskDto.Priority;
            task.DueDate = taskDto.DueDate;
            task.Description = taskDto.Description;

            // Check if the status has changed.
            // Status değişti mi kontrol et
            if (oldStatus != taskDto.Status)
            {
                task.Status = taskDto.Status;

                if (taskDto.Status == AuditTaskStatus.COMPLETED)
                {
                    task.CompletedAt = DateTime.UtcNow;
                }
                else
                {
                    task.CompletedAt = null;
                }
            }

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
