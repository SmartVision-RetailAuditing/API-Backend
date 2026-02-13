using ApiBackend.DTOs;
using ApiBackend.DTOs.LoginDtos;
using ApiBackend.DTOs.StatsDtos;
using ApiBackend.DTOs.UserDtos;
using ApiBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            int userId = int.Parse(userIdString);

            var profile = await _userService.GetUserProfileAsync(userId);

            if (profile == null) return NotFound(new { message = "Profile not found." });

            return Ok(profile);
        }

        [HttpGet("me/stats")]
        public async Task<ActionResult<UserStatsResponseDto>> GetMyStats()
        {
            // Token'dan User ID'yi al
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            int userId = int.Parse(userIdString);

            // Servisi çağır
            var stats = await _userService.GetUserStatsAsync(userId);

            return Ok(stats);
        }

        // GET: api/users (List All)
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var users = await _userService.GetAllUsersAsync(page, size);
            return Ok(users);
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")] // Detayı kimler görebilsin?
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(user);
        }

        // GET: api/users/field-workers
        // Amaç: Görev atama ekranında dropdown doldurmak
        [HttpGet("field-workers")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")] // Supervisor'lar buraya erişebilir!
        public async Task<ActionResult<IEnumerable<UserDto>>> GetFieldWorkers()
        {
            var workers = await _userService.GetFieldWorkersAsync();
            return Ok(workers);
        }

        // POST: api/users (Create User)
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto request)
        {
            var createdUser = await _userService.CreateUserAsync(request);
            // 1.Status Code: 201 Created döner.
            // 2. Header: Location: api/users/5 bilgisini ekler.
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }

        // PUT: api/users/5 (Update User)
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto request)
        {
            var result = await _userService.UpdateUserAsync(id, request);
            if (!result) return NotFound(new { message = "User not found." });
            return NoContent();
        }

        // DELETE: api/users/5 (Delete User)
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result) return NotFound(new { message = "User not found." });
            return NoContent();
        }

        // PATCH: api/users/5/reset-password
        // Admin, "User Management" sayfasında bir kullanıcıya tıklayıp şifresini sıfırlar.
        [HttpPatch("{id}/reset-password")]
        [Authorize(Roles = "ADMIN")] // Sadece Admin yapabilir
        public async Task<IActionResult> AdminResetPassword(int id, [FromBody] AdminResetPasswordDto request)
        {
            var result = await _userService.AdminResetPasswordAsync(id, request.NewPassword);

            if (!result) return NotFound(new { message = "User not found." });

            return Ok(new { message = "User password successfully reset." });
        }
    }
}