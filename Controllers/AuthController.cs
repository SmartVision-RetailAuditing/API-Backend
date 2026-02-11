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
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
        {
            var response = await _authService.LoginAsync(request);

            if (response == null)
            {
                return Unauthorized(new { message = "Email or password is incorrect." });
            }

            return Ok(response);
        }

        // POST: api/auth/change-password
        [HttpPost("change-password")]
        [Authorize] // Anyone logged in can change his password.
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            // Token'dan ID'yi al (Baþkasý adýna deðiþtiremesin diye)
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            int userId = int.Parse(userIdString);

            var result = await _authService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);

            if (!result) return BadRequest("The old password was incorrect or the user could not be found.");

            return Ok(new { message = "Your password has been successfully changed." });
        }
    }
}
