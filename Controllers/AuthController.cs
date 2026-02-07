using ApiBackend.Data;
using ApiBackend.DTOs.LoginDtos;
using BCrypt.Net; // Þifre kontrolü için
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace ApiBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
        {
            // 1. Kullanýcýyý emaile göre bul
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "Email veya þifre hatalý." });
            }

            // 2. Þifreyi kontrol et (Hash doðrulamasý)
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Email veya þifre hatalý." });
            }

            // 3. JWT Token Oluþtur
            var token = CreateToken(user);

            // 4. Cevabý dön
            return Ok(new LoginResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role.ToString()
                }
            });
        }

        // Token üretim metodu
        private string CreateToken(Entities.User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()) // Rolü tokena gömüyoruz
            };

            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(jwtSettings["DurationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
