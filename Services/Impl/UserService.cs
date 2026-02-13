using ApiBackend.DTOs;
using ApiBackend.DTOs.StatsDtos;
using ApiBackend.DTOs.UserDtos;
using ApiBackend.Repositories.Interfaces;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    /// <summary>
    /// Service layer for user-related business logic and operations
    /// Kullanıcı ile ilgili iş mantığı ve işlemleri için servis katmanı
    /// </summary>
    public class UserService : IUserService
    {
        // Repository dependencies for data access
        // Veri erişimi için repository bağımlılıkları
        private readonly IUserRepository _userRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IAuditRepository _auditRepository;

        /// <summary>
        /// Constructor with dependency injection for user, task, and audit repositories
        /// Kullanıcı, görev ve denetim repository'leri için dependency injection ile constructor
        /// </summary>
        public UserService(IUserRepository userRepository, ITaskRepository taskRepository, IAuditRepository auditRepository)
        {
            _userRepository = userRepository;
            _taskRepository = taskRepository;
            _auditRepository = auditRepository;
        }

        /// <summary>
        /// Retrieves user profile with statistics
        /// Kullanıcı profilini istatistikler ile getirir
        /// </summary>
        /// <param name="userId">User ID / Kullanıcı ID'si</param>
        /// <returns>User profile DTO with stats / İstatistiklerle birlikte kullanıcı profili DTO'su</returns>
        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
        {
            // Step 1: Find the user by ID
            // Adım 1: Kullanıcıyı ID ile bul
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return null;

            // Step 2: Get user's tasks for statistics calculation
            // Adım 2: İstatistik hesaplaması için kullanıcının görevlerini getir
            var userTasks = await _taskRepository.GetTasksByUserIdAsync(userId);

            // Kullanıcının yaptığı denetimleri getir
            var userAudits = await _auditRepository.GetAuditsByUserIdAsync(userId);

            // Step 3: Calculate statistics (Business Logic)
            // Adım 3: İstatistikleri hesapla (İş Mantığı)

            // Total number of tasks assigned to user
            // Kullanıcıya atanan toplam görev sayısı
            int totalTasks = userTasks.Count();

            // Number of completed tasks
            // Tamamlanan görev sayısı
            int completedTasks = userTasks.Count(t => t.Status == Entities.AuditTaskStatus.COMPLETED);

            // Number of pending tasks
            // Bekleyen görev sayısı
            int pendingTasks = totalTasks - completedTasks;

            // Calculate completion rate as percentage
            // Tamamlanma oranını yüzde olarak hesapla
            int completionRate = totalTasks > 0
                ? (int)((double)completedTasks / totalTasks * 100)
                : 0;

            // Average Score Hesaplama (GERÇEK VERİ)
            int avgScore = 0;
            if (userAudits.Any())
            {
                // ComplianceScore decimal olduğu için int'e cast ediyoruz veya Math.Round kullanıyoruz
                avgScore = (int)Math.Round(userAudits.Average(a => a.ComplianceScore));
            }

            // Step 4: Create and return DTO
            // Adım 4: DTO oluştur ve dön
            return new UserProfileDto
            {
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                EmployeeId = user.EmployeeId ?? "N/A",
                Phone = user.Phone ?? "",
                Stats = new UserStatsDto
                {
                    TotalTasks = totalTasks,
                    CompletedTasks = completedTasks,
                    PendingTasks = pendingTasks,
                    TotalStoreVisits = completedTasks, // Each completed task = 1 visit / Her tamamlanan görev = 1 ziyaret
                    AverageScore = avgScore,
                    CompletionRate = completionRate
                }
            };
        }

        /// <summary>
        /// Retrieves detailed user statistics including weekly tasks and performance metrics
        /// Haftalık görevler ve performans metrikleri dahil detaylı kullanıcı istatistiklerini getirir
        /// </summary>
        /// <param name="userId">User ID / Kullanıcı ID'si</param>
        /// <returns>Complete user statistics / Kapsamlı kullanıcı istatistikleri</returns>
        public async Task<UserStatsResponseDto> GetUserStatsAsync(int userId)
        {
            // Step 1: Fetch data from repositories
            // Adım 1: Repository'lerden verileri çek
            var allTasks = await _taskRepository.GetTasksByUserIdAsync(userId);
            var audits = await _auditRepository.GetAuditsByUserIdAsync(userId);

            // Step 2: WEEKLY STATISTICS (WeeklyTasks)
            // Adım 2: HAFTALIK İSTATİSTİKLER (WeeklyTasks)

            // Get the start of current week (Monday)
            // Bu haftanın başlangıcını al (Pazartesi)
            var today = DateTime.UtcNow;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

            // Filter tasks that belong to current week
            // Bu haftaya ait görevleri filtrele
            var weeklyTasks = allTasks.Where(t => t.DueDate >= startOfWeek).ToList();

            // Calculate weekly task counts
            // Haftalık görev sayılarını hesapla
            int totalWeekly = weeklyTasks.Count;
            int completedWeekly = weeklyTasks.Count(t => t.Status == Entities.AuditTaskStatus.COMPLETED);
            int pendingWeekly = totalWeekly - completedWeekly;

            // Step 3: PERFORMANCE STATISTICS (Performance)
            // Adım 3: PERFORMANS İSTATİSTİKLERİ (Performance)

            // Completion rate (All time)
            // Tamamlama oranı (Tüm zamanlar)
            int totalAllTime = allTasks.Count();
            int completedAllTime = allTasks.Count(t => t.Status == Entities.AuditTaskStatus.COMPLETED);

            // Calculate overall completion rate as percentage
            // Genel tamamlanma oranını yüzde olarak hesapla
            int completionRate = totalAllTime > 0
                ? (int)((double)completedAllTime / totalAllTime * 100)
                : 0;

            // Average score (from Audit table compliance scores)
            // Ortalama puan (Audit tablosundaki uyumluluk puanlarından)
            int avgScore = 0;
            if (audits.Any())
            {
                avgScore = (int)audits.Average(a => a.ComplianceScore);
            }

            // Step 4: Return the result
            // Adım 4: Sonucu dön
            return new UserStatsResponseDto
            {
                WeeklyTasks = new WeeklyTasksStatsDto
                {
                    Total = totalWeekly,
                    Completed = completedWeekly,
                    Pending = pendingWeekly
                },
                Performance = new PerformanceStatsDto
                {
                    CompletionRate = completionRate,
                    AverageScore = avgScore,
                    TotalStoreVisits = completedAllTime // Each completed task counted as 1 visit / Her tamamlanan görev 1 ziyaret olarak sayıldı
                }
            };
        }

        /// <summary>
        /// Retrieves all users from the system (with pagination)
        /// Sistemdeki tüm kullanıcıları getirir (sayfalama ile)
        /// </summary>
        /// <param name="page">Page number for pagination / Sayfalama için sayfa numarası</param>
        /// <param name="size">Number of items per page / Sayfa başına öğe sayısı</param>
        /// <returns>List of user DTOs / Kullanıcı DTO'larının listesi</returns>
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(int page = 1, int size = 10)
        {
            // Fetch users from repository with pagination
            // Repository'den kullanıcıları sayfalama ile çek
            var users = await _userRepository.GetAllUsersAsync(page, size);

            // Map entities to DTOs using LINQ projection
            // Entity'leri LINQ projeksiyonu kullanarak DTO'lara dönüştür
            return users.Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.ToString(),
                EmployeeId = u.EmployeeId,
                Phone = u.Phone
            });
        }

        /// <summary>
        /// Retrieves a specific user by ID
        /// Belirli bir kullanıcıyı ID ile getirir
        /// </summary>
        /// <param name="id">User ID / Kullanıcı ID'si</param>
        /// <returns>User DTO or null if not found / Kullanıcı DTO'su veya bulunamazsa null</returns>
        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            // Find user by ID
            // Kullanıcıyı ID ile bul
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return null;

            // Map entity to DTO
            // Entity'yi DTO'ya dönüştür
            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                EmployeeId = user.EmployeeId,
                Phone = user.Phone
            };
        }


        /// <summary>
        /// Creates a new user in the system
        /// Sistemde yeni bir kullanıcı oluşturur
        /// </summary>
        /// <param name="userDto">User creation DTO / Kullanıcı oluşturma DTO'su</param>
        /// <returns>Created user DTO / Oluşturulan kullanıcı DTO'su</returns>
        public async Task<UserDto> CreateUserAsync(CreateUserDto userDto)
        {
            // Hash the password for security using BCrypt
            // Güvenlik için şifreyi BCrypt kullanarak hashle
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            // Convert string role to enum
            // String rol değerini enum'a dönüştür
            Enum.TryParse(userDto.Role, out Entities.UserRole roleEnum);

            // Create new user entity
            // Yeni kullanıcı entity'si oluştur
            var user = new Entities.User
            {
                FullName = userDto.FullName,
                Email = userDto.Email,
                PasswordHash = passwordHash, // Store hashed password, never plain text / Hashlenmiş şifreyi sakla, asla düz metin olarak saklanmaz
                Role = roleEnum,
                EmployeeId = userDto.EmployeeId,
                Phone = userDto.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Save user to database
            // Kullanıcıyı veritabanına kaydet
            await _userRepository.AddUserAsync(user);

            // Return created user as DTO
            // Oluşturulan kullanıcıyı DTO olarak dön
            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                EmployeeId = user.EmployeeId,
                Phone = user.Phone
            };
        }

        /// <summary>
        /// Updates an existing user's information
        /// Mevcut bir kullanıcının bilgilerini günceller
        /// </summary>
        /// <param name="id">User ID / Kullanıcı ID'si</param>
        /// <param name="userDto">Updated user data / Güncellenmiş kullanıcı verisi</param>
        /// <returns>True if successful, false if user not found / Başarılıysa true, kullanıcı bulunamazsa false</returns>
        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto userDto)
        {
            // Find user by ID
            // Kullanıcıyı ID ile bul
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return false;


            // Convert string role to enum
            // String rol değerini enum'a dönüştür
            Enum.TryParse(userDto.Role, out Entities.UserRole roleEnum);

            // Update user properties
            // Kullanıcı özelliklerini güncelle
            user.FullName = userDto.FullName;
            user.Role = roleEnum;
            user.EmployeeId = userDto.EmployeeId;
            user.Phone = userDto.Phone;
            user.IsActive = userDto.IsActive;

            // Save changes to database
            // Değişiklikleri veritabanına kaydet
            await _userRepository.UpdateUserAsync(user);
            return true;
        }

        /// <summary>
        /// Deletes a user from the system
        /// Sistemden bir kullanıcıyı siler
        /// </summary>
        /// <param name="id">User ID / Kullanıcı ID'si</param>
        /// <returns>True if successful, false if user not found / Başarılıysa true, kullanıcı bulunamazsa false</returns>
        public async Task<bool> DeleteUserAsync(int id)
        {
            // Find user by ID
            // Kullanıcıyı ID ile bul
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return false;

            // Delete user from database
            // Kullanıcıyı veritabanından sil
            await _userRepository.DeleteUserAsync(user);
            return true;
        }

        /// <summary>
        /// Admin function to reset a user's password
        /// Bir kullanıcının şifresini sıfırlamak için admin fonksiyonu
        /// </summary>
        /// <param name="userId">User ID / Kullanıcı ID'si</param>
        /// <param name="newPassword">New password (plain text) / Yeni şifre (düz metin)</param>
        /// <returns>True if successful, false if user not found / Başarılıysa true, kullanıcı bulunamazsa false</returns>
        public async Task<bool> AdminResetPasswordAsync(int userId, string newPassword)
        {
            // Find user by ID
            // Kullanıcıyı ID ile bul
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return false;

            // Hash the new password and save
            // Yeni şifreyi hashle ve kaydet
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // Use existing repository UpdateUserAsync method to save
            // Kaydetmek için mevcut repository UpdateUserAsync metodunu kullan
            await _userRepository.UpdateUserAsync(user);

            return true;
        }

        /// <summary>
        /// Retrieves all field workers from the system
        /// Sistemdeki tüm saha çalışanlarını getirir
        /// </summary>
        /// <returns>List of field worker DTOs / Saha çalışanı DTO'larının listesi</returns>
        public async Task<IEnumerable<UserDto>> GetFieldWorkersAsync()
        {
            // Call repository with FIELD_WORKER enum
            // Repository'i FIELD_WORKER enum'ı ile çağır
            var workers = await _userRepository.GetUsersByRoleAsync(Entities.UserRole.FIELD_WORKER);

            // Map entities to DTOs
            // Entity'leri DTO'lara dönüştür
            return workers.Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.ToString(),
                EmployeeId = u.EmployeeId,
                Phone = u.Phone
            });
        }
    }
}
