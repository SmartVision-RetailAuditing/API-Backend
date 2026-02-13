using ApiBackend.DTOs;
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
    /// <summary>
    /// Service layer for authentication and authorization operations
    /// Kimlik doğrulama ve yetkilendirme işlemleri için servis katmanı
    /// </summary>
    public class AuthService : IAuthService
    {
        // User repository for database access
        // Veritabanı erişimi için kullanıcı repository'si
        private readonly IUserRepository _userRepository;

        // Configuration for JWT settings
        // JWT ayarları için konfigürasyon
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor with dependency injection for user repository and configuration
        /// Kullanıcı repository'si ve konfigürasyon için dependency injection ile constructor
        /// </summary>
        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        /// <summary>
        /// Authenticates user and returns JWT token
        /// Kullanıcıyı doğrular ve JWT token döner
        /// </summary>
        /// <param name="request">Login request with email and password / Email ve şifre içeren giriş isteği</param>
        /// <returns>Login response with token and user info, or null if authentication fails / Token ve kullanıcı bilgisi içeren giriş yanıtı, veya doğrulama başarısızsa null</returns>
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            // Logic 1: Check if user exists
            // Mantık 1: Kullanıcı var mı kontrol et
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null) return null;

            // Logic 2: Verify password using BCrypt
            // Mantık 2: BCrypt kullanarak şifreyi doğrula
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return null;

            // Logic 3: Generate JWT token
            // Mantık 3: JWT token üret
            var token = CreateToken(user);

            // Return successful login response with token only (user info removed)
            // Token ile başarılı giriş yanıtı dön (kullanıcı bilgisi kaldırıldı)
            return new LoginResponseDto
            {
                Token = token,
                //User = new UserDto
                //{
                //    Id = user.Id,
                //    FullName = user.FullName,
                //    Email = user.Email,
                //    Role = user.Role.ToString()
                //}
            };
        }

        /// <summary>
        /// Creates a JWT token for authenticated user
        /// Doğrulanmış kullanıcı için JWT token oluşturur
        /// </summary>
        /// <param name="user">Authenticated user entity / Doğrulanmış kullanıcı entity'si</param>
        /// <returns>JWT token string / JWT token string'i</returns>
        private string CreateToken(User user)
        {
            // Get JWT settings from configuration
            // Konfigürasyondan JWT ayarlarını al
            var jwtSettings = _configuration.GetSection("JwtSettings");

            // Convert secret key to bytes
            // Gizli anahtarı byte'lara dönüştür
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            // Create claims (user information to be embedded in token)
            // Claims oluştur (token içine gömülecek kullanıcı bilgileri)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // User ID / Kullanıcı ID'si
                new Claim(ClaimTypes.Email, user.Email),                   // User email / Kullanıcı email'i
                new Claim(ClaimTypes.Role, user.Role.ToString())           // User role for authorization / Yetkilendirme için kullanıcı rolü
            };

            // Create signing credentials using HMAC SHA256 algorithm
            // HMAC SHA256 algoritması kullanarak imzalama kimlik bilgilerini oluştur
            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            // Get token duration from configuration
            // Konfigürasyondan token geçerlilik süresini al
            var duration = _configuration.GetValue<double>("JwtSettings:DurationInMinutes");

            // Create JWT token with all parameters
            // Tüm parametrelerle JWT token oluştur
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],           // Token issuer / Token yayıncısı
                audience: jwtSettings["Audience"],       // Token audience / Token hedef kitlesi
                claims: claims,                          // User claims / Kullanıcı claims'leri
                expires: DateTime.UtcNow.AddMinutes(duration), // Expiration time / Geçerlilik süresi
                signingCredentials: creds                // Signing credentials / İmzalama kimlik bilgileri
            );

            // Convert token to string and return
            // Token'ı string'e dönüştür ve dön
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Changes user's password after verifying old password
        /// Eski şifreyi doğruladıktan sonra kullanıcının şifresini değiştirir
        /// </summary>
        /// <param name="userId">User ID / Kullanıcı ID'si</param>
        /// <param name="oldPassword">Current password / Mevcut şifre</param>
        /// <param name="newPassword">New password / Yeni şifre</param>
        /// <returns>True if successful, false if user not found or old password is incorrect / Başarılıysa true, kullanıcı bulunamazsa veya eski şifre yanlışsa false</returns>
        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            // Find user by ID
            // Kullanıcıyı ID ile bul
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return false;

            // Step 1: Verify old password is correct
            // Adım 1: Eski şifrenin doğru olup olmadığını kontrol et
            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            {
                return false; // Old password is incorrect / Eski şifre yanlış
            }

            // Step 2: Hash the new password
            // Adım 2: Yeni şifreyi hashle
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // Step 3: Save updated user to database
            // Adım 3: Güncellenmiş kullanıcıyı veritabanına kaydet
            await _userRepository.UpdateUserAsync(user);

            return true;
        }
    }
}
