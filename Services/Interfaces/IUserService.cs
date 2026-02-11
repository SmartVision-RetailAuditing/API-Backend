using ApiBackend.DTOs.LoginDtos;
using ApiBackend.DTOs.StatsDtos;
using ApiBackend.DTOs.UserDtos;

namespace ApiBackend.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetUserProfileAsync(int userId);
        Task<UserStatsResponseDto> GetUserStatsAsync(int userId);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> CreateUserAsync(CreateUserDto userDto);
        Task<bool> UpdateUserAsync(int id, UpdateUserDto userDto);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> AdminResetPasswordAsync(int userId, string newPassword);
    }
}
