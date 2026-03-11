using System.ComponentModel.DataAnnotations;
using ApiBackend.Entities;

namespace ApiBackend.DTOs.UserDtos
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "FullName is required")]
        [MinLength(1, ErrorMessage = "FullName cannot be empty")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [EnumDataType(typeof(UserRole), ErrorMessage = "Role must be ADMIN, SUPERVISOR, or FIELD_WORKER")]
        public string Role { get; set; }

        public string? EmployeeId { get; set; }

        [Phone(ErrorMessage = "Phone number is invalid")]
        public string? Phone { get; set; }
    }
}
