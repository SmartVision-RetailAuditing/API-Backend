using System.ComponentModel.DataAnnotations;


namespace ApiBackend.DTOs.LoginDtos
{
    // Senaryo: Kullanıcı kendi şifresini değiştiriyor (Eskiyi bilmek zorunda)
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Eski şifre zorunludur.")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New password is required..")]
        [MinLength(6, ErrorMessage = "The password must be at least 6 characters long..")]
        public string NewPassword { get; set; }
    }
}
