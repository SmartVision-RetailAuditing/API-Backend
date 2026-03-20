using ApiBackend.Entities;

namespace ApiBackend.DTOs.LoginDtos
{
    public class LoginResponseDto
    {
        public string Token { get; set; } // JWT
        public int UserId { get; set; }
        public string Role { get; set; }
        
    }
}
