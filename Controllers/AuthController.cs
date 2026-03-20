using ApiBackend.DTOs.LoginDtos;
using ApiBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IEventPublisher _eventPublisher;
        public AuthController(IAuthService authService, IEventPublisher eventPublisher)
        {
            _authService = authService;
            _eventPublisher = eventPublisher;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            var response = await _authService.LoginAsync(request);

            if (response == null)
            {
                return Unauthorized(new { message = "Email or password is incorrect." });
            }
            await _eventPublisher.PublishUserLoggedInAsync(response.UserId, DateTime.UtcNow);

            return Ok(response);
        }

        // POST: api/auth/change-password
        [HttpPost("change-password")]
        [Authorize] // Anyone logged in can change his password.
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            // Token'dan ID'yi al (Ba�kas� ad�na de�i�tiremesin diye)
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            int userId = int.Parse(userIdString);

            var result = await _authService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);

            if (!result) return BadRequest(new { message = "Old password is wrong or user not found." });

            return Ok(new { message = "Password updated successfully." });
        }
    }
    
    
}
