using ApiBackend.DTOs;
using ApiBackend.DTOs.StatsDtos;
using ApiBackend.DTOs.UserDtos;
using ApiBackend.Repositories.Interfaces;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IAuditRepository _auditRepository;

        public UserService(
            IUserRepository userRepository,
            ITaskRepository taskRepository,
            IAuditRepository auditRepository)
        {
            _userRepository = userRepository;
            _taskRepository = taskRepository;
            _auditRepository = auditRepository;
        }

        // ── List ──────────────────────────────────────────────────────────────

        public async Task<PagedResult<UserDto>> GetAllUsersAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            string? role = null)
        {
            return await _userRepository.GetAllUsersAsync(pageNumber, pageSize, search, role);
        }

        // ── Single ────────────────────────────────────────────────────────────

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                EmployeeId = user.EmployeeId,
                Phone = user.Phone,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin,
            };
        }

        // ── Profile & Stats ───────────────────────────────────────────────────

        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return null!;

            var userTasks = await _taskRepository.GetTasksByUserIdAsync(userId);
            var userAudits = await _auditRepository.GetAuditsByUserIdAsync(userId);

            int totalTasks = userTasks.Count();
            int completedTasks = userTasks.Count(t => t.Status == Entities.AuditTaskStatus.COMPLETED);
            int pendingTasks = totalTasks - completedTasks;
            int completionRate = totalTasks > 0
                ? (int)((double)completedTasks / totalTasks * 100) : 0;
            int avgScore = userAudits.Any()
                ? (int)Math.Round(userAudits.Average(a => a.ComplianceScore)) : 0;

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
                    TotalStoreVisits = completedTasks,
                    AverageScore = avgScore,
                    CompletionRate = completionRate,
                }
            };
        }

        public async Task<UserStatsResponseDto> GetUserStatsAsync(int userId)
        {
            var allTasks = await _taskRepository.GetTasksByUserIdAsync(userId);
            var audits = await _auditRepository.GetAuditsByUserIdAsync(userId);

            var today = DateTime.UtcNow;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            var weeklyTasks = allTasks.Where(t => t.DueDate >= startOfWeek).ToList();

            int totalWeekly = weeklyTasks.Count;
            int completedWeekly = weeklyTasks.Count(t => t.Status == Entities.AuditTaskStatus.COMPLETED);

            int totalAllTime = allTasks.Count();
            int completedAllTime = allTasks.Count(t => t.Status == Entities.AuditTaskStatus.COMPLETED);
            int completionRate = totalAllTime > 0
                ? (int)((double)completedAllTime / totalAllTime * 100) : 0;
            int avgScore = audits.Any()
                ? (int)audits.Average(a => a.ComplianceScore) : 0;

            return new UserStatsResponseDto
            {
                WeeklyTasks = new WeeklyTasksStatsDto
                {
                    Total = totalWeekly,
                    Completed = completedWeekly,
                    Pending = totalWeekly - completedWeekly,
                },
                Performance = new PerformanceStatsDto
                {
                    CompletionRate = completionRate,
                    AverageScore = avgScore,
                    TotalStoreVisits = completedAllTime,
                }
            };
        }

        // ── Create / Update / Delete ──────────────────────────────────────────

        public async Task<UserDto> CreateUserAsync(CreateUserDto userDto)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            Enum.TryParse(userDto.Role, out Entities.UserRole roleEnum);

            var user = new Entities.User
            {
                FullName = userDto.FullName,
                Email = userDto.Email,
                PasswordHash = passwordHash,
                Role = roleEnum,
                EmployeeId = userDto.EmployeeId,
                Phone = userDto.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            await _userRepository.AddUserAsync(user);

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                EmployeeId = user.EmployeeId,
                Phone = user.Phone,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin,
            };
        }

        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto userDto)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return false;

            Enum.TryParse(userDto.Role, out Entities.UserRole roleEnum);

            user.FullName = userDto.FullName ?? user.FullName;
            user.Role = roleEnum;
            user.EmployeeId = userDto.EmployeeId;
            user.Phone = userDto.Phone;
            user.IsActive = userDto.IsActive;

            await _userRepository.UpdateUserAsync(user);
            return true;
        }

        // Soft delete toggle — IsActive'i tersine çevirir
        public async Task<bool> ToggleUserActiveAsync(int userId)
        {
            return await _userRepository.ToggleUserActiveAsync(userId);
        }

        public async Task<bool> AdminResetPasswordAsync(int userId, string newPassword)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepository.UpdateUserAsync(user);
            return true;
        }

        // AssignTaskModal için — search destekli, sadece aktif FIELD_WORKER'lar
        public async Task<IEnumerable<UserDto>> GetFieldWorkersAsync(string? search = null)
        {
            return await _userRepository.GetFieldWorkersAsync(search);
        }
    }
}
