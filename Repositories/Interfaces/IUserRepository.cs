using ApiBackend.DTOs;
using ApiBackend.DTOs.UserDtos;
using ApiBackend.Entities;

namespace ApiBackend.Repositories.Interfaces
{
    public interface IUserRepository
    {
        // Pagination + search + role filter — Task backend'indeki pattern ile aynı
        Task<PagedResult<UserDto>> GetAllUsersAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            string? role = null
        );

        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int id);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);

        // Soft delete: IsActive = false — hard delete kaldırıldı
        Task<bool> ToggleUserActiveAsync(int userId);

        // AssignTaskModal için — sadece aktif FIELD_WORKER'lar, search destekli
        Task<IEnumerable<UserDto>> GetFieldWorkersAsync(string? search = null);
    }
}