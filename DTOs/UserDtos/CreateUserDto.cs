using System.ComponentModel.DataAnnotations;

namespace ApiBackend.DTOs.UserDtos
{
    public class CreateUserDto
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(6)]
        public string Password { get; set; } // Create ederken şifre lazım
        [Required]
        public string Role { get; set; } // "ADMIN", "SUPERVISOR", "FIELD_WORKER"
        public string? EmployeeId { get; set; }
        public string? Phone { get; set; }
    }
}
