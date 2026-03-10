using ApiBackend.Data;
using ApiBackend.DTOs;
using ApiBackend.DTOs.UserDtos;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Repositories.Impl
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<UserDto>> GetAllUsersAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            string? role = null)
        {
            var query = _context.Users.AsQueryable();

            // Search: FullName, Email veya EmployeeId üzerinden
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(lower) ||
                    u.Email.ToLower().Contains(lower) ||
                    (u.EmployeeId != null && u.EmployeeId.ToLower().Contains(lower))
                );
            }

            // Role filtresi
            if (!string.IsNullOrWhiteSpace(role) &&
                Enum.TryParse<UserRole>(role.ToUpper(), out var parsedRole))
            {
                query = query.Where(u => u.Role == parsedRole);
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderBy(u => u.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    EmployeeId = u.EmployeeId,
                    Phone = u.Phone,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastLogin = u.LastLogin,
                })
                .ToListAsync();

            return new PagedResult<UserDto>
            {
                Data = data,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = pageNumber,
                PageSize = pageSize,
            };
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        // Soft delete — IsActive toggle
        public async Task<bool> ToggleUserActiveAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        // AssignTaskModal için — sadece aktif FIELD_WORKER'lar, opsiyonel search
        public async Task<IEnumerable<UserDto>> GetFieldWorkersAsync(string? search = null)
        {
            var query = _context.Users
                .Where(u => u.Role == UserRole.FIELD_WORKER && u.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(lower) ||
                    u.Email.ToLower().Contains(lower)
                );
            }

            return await query
                .OrderBy(u => u.FullName)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    EmployeeId = u.EmployeeId,
                    Phone = u.Phone,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastLogin = u.LastLogin,
                })
                .ToListAsync();
        }
    }
}