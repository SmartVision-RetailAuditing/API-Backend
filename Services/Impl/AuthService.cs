using ApiBackend.DTOs.LoginDtos;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using ApiBackend.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiBackend.Services.Impl
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null) return null!;

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return null!;

            // Son giriş zamanını güncelle
            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateUserAsync(user);

            var token = CreateToken(user);

            return new LoginResponseDto 
            { 
                Token = token,
                UserId = user.Id,
                Role = user.Role.ToString() // .ToString() ekleyerek string'e çeviriyoruz
            };
        }

        private string CreateToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email,           user.Email),
                new Claim(ClaimTypes.Role,            user.Role.ToString()),
                new Claim(ClaimTypes.Name,            user.FullName),  // YENİ — TopBar için
            };

            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var duration = _configuration.GetValue<double>("JwtSettings:DurationInMinutes");

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(duration),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash)) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepository.UpdateUserAsync(user);
            return true;
        }
    }
}