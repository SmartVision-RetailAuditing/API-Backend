namespace ApiBackend.DTOs.UserDtos
{
    public class UpdateUserDto
    {
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public string? EmployeeId { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
    }
}
