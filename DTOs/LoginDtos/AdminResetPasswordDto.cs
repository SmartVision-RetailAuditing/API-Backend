using System.ComponentModel.DataAnnotations;

namespace ApiBackend.DTOs.LoginDtos
{
    // Senaryo: Admin, kullanıcının şifresini zorla değiştiriyor (Eskiyi bilmesine gerek yok)
    public class AdminResetPasswordDto
    {
        [Required(ErrorMessage = "New password is required.")]
        [MinLength(6, ErrorMessage = "The password must be at least 6 characters long.")]
        public string NewPassword { get; set; }
    }
}
