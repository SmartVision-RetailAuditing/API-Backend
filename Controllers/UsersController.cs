using ApiBackend.DTOs;
using ApiBackend.DTOs.LoginDtos;
using ApiBackend.DTOs.StatsDtos;
using ApiBackend.DTOs.UserDtos;
using ApiBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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

        // GET: api/users/profile
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            var profile = await _userService.GetUserProfileAsync(int.Parse(userIdString));
            if (profile == null) return NotFound(new { message = "Profile not found." });

            return Ok(profile);
        }

        // GET: api/users/me/stats
        [HttpGet("me/stats")]
        public async Task<ActionResult<UserStatsResponseDto>> GetMyStats()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            var stats = await _userService.GetUserStatsAsync(int.Parse(userIdString));
            return Ok(stats);
        }

        // GET: api/users?page=1&size=10&search=ahmet&role=FIELD_WORKER
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<PagedResult<UserDto>>> GetAllUsers(
            [FromQuery, Range(1, int.MaxValue)] int page = 1,
            [FromQuery, Range(1, 100)] int size = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? role = null)
        {
            var result = await _userService.GetAllUsersAsync(page, size, search, role);
            return Ok(result);
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<UserDto>> GetUserById([Range(1, int.MaxValue)] int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });
            return Ok(user);
        }

        // GET: api/users/field-workers?search=ahmet
        // AssignTaskModal dropdown'u için — ADMIN ve SUPERVISOR erişebilir
        [HttpGet("field-workers")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetFieldWorkers(
            [FromQuery] string? search = null)
        {
            var workers = await _userService.GetFieldWorkersAsync(search);
            return Ok(workers);
        }

        // POST: api/users
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto request)
        {
            var created = await _userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUserById), new { id = created.Id }, created);
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateUser(
                [Range(1, int.MaxValue)] int id,
                [FromBody] UpdateUserDto request)
        {
            var result = await _userService.UpdateUserAsync(id, request);
            if (!result) return NotFound(new { message = "User not found." });
            return NoContent();
        }

        // PATCH: api/users/5/toggle-active  — soft delete toggle
        [HttpPatch("{id}/toggle-active")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ToggleUserActive([Range(1, int.MaxValue)] int id)
        {
            var result = await _userService.ToggleUserActiveAsync(id);
            if (!result) return NotFound(new { message = "User not found." });
            return Ok(new { message = "User active status toggled." });
        }

        // PATCH: api/users/5/reset-password
        [HttpPatch("{id}/reset-password")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> AdminResetPassword(
                [Range(1, int.MaxValue)] int id,
                [FromBody] AdminResetPasswordDto request)
        {
            var result = await _userService.AdminResetPasswordAsync(id, request.NewPassword);
            if (!result) return NotFound(new { message = "User not found." });
            return Ok(new { message = "User password successfully reset." });
        }
    }
}
