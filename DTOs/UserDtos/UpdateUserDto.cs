using System.ComponentModel.DataAnnotations;
using ApiBackend.Entities;
namespace ApiBackend.DTOs.UserDtos
{
    public class UpdateUserDto
    {
        public string? FullName { get; set; }

        [EnumDataType(typeof(UserRole), ErrorMessage = "Role must be ADMIN, SUPERVISOR, or FIELD_WORKER")]
        public string? Role { get; set; }

        public string? EmployeeId { get; set; }

        [Phone(ErrorMessage = "Phone number is invalid")]
        public string? Phone { get; set; }

        public bool IsActive { get; set; } // make nullable to know if client intends to update it
    }
}
