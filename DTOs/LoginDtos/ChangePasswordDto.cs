namespace ApiBackend.DTOs.LoginDtos
{
    // Senaryo: Kullanıcı kendi şifresini değiştiriyor (Eskiyi bilmek zorunda)
    public class ChangePasswordDto
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
