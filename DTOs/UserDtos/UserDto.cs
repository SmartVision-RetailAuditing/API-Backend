namespace ApiBackend.DTOs.UserDtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string? EmployeeId { get; set; }
        public string? Phone { get; set; }
    }
}
