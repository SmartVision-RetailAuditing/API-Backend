using ApiBackend.DTOs.LoginDtos;

namespace ApiBackend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    }
}
