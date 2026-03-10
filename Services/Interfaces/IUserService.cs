using ApiBackend.DTOs;
using ApiBackend.DTOs.StatsDtos;
using ApiBackend.DTOs.UserDtos;

namespace ApiBackend.Services.Interfaces
{
    public interface IUserService
    {
        // Pagination + search + role filter
        Task<PagedResult<UserDto>> GetAllUsersAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            string? role = null
        );

        Task<UserProfileDto> GetUserProfileAsync(int userId);
        Task<UserStatsResponseDto> GetUserStatsAsync(int userId);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto> CreateUserAsync(CreateUserDto userDto);
        Task<bool> UpdateUserAsync(int id, UpdateUserDto userDto);

        // Soft delete toggle
        Task<bool> ToggleUserActiveAsync(int userId);

        Task<bool> AdminResetPasswordAsync(int userId, string newPassword);

        // AssignTaskModal için — search destekli
        Task<IEnumerable<UserDto>> GetFieldWorkersAsync(string? search = null);
    }
}