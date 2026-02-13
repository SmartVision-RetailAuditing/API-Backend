using ApiBackend.Entities;

namespace ApiBackend.DTOs.LoginDtos
{
    public class LoginResponseDto
    {
        public string Token { get; set; } // JWT
        // public UserDto User { get; set; } // Response sadece token dönsün.
    }
}
