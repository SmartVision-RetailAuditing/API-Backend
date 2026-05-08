namespace ApiBackend.DTOs.UserDtos
{
    public class UserProfileDto
    {
        public string FullName { get; set; }
        public string? EmployeeId { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }

        // Mobil Profil Sayfasındaki Kartlar
        public UserStatsDto Stats { get; set; }
    }
}
