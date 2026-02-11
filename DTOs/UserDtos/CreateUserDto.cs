namespace ApiBackend.DTOs.UserDtos
{
    public class CreateUserDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // Create ederken şifre lazım
        public string Role { get; set; } // "ADMIN", "SUPERVISOR", "FIELD_WORKER"
        public string? EmployeeId { get; set; }
        public string? Phone { get; set; }
    }
}
