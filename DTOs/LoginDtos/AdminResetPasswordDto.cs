namespace ApiBackend.DTOs.LoginDtos
{
    // Senaryo: Admin, kullanıcının şifresini zorla değiştiriyor (Eskiyi bilmesine gerek yok)
    public class AdminResetPasswordDto
    {
        public string NewPassword { get; set; }
    }
}
