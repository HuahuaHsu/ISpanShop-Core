using System.ComponentModel.DataAnnotations;

namespace ISpanShop.Models.DTOs.Auth
{
    public class FrontRegisterRequestDto
    {
        [Required(ErrorMessage = "帳號為必填")]
        public string Account { get; set; } = string.Empty;

        [Required(ErrorMessage = "密碼為必填")]
        [MinLength(6, ErrorMessage = "密碼長度至少為 6 個字元")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email 為必填")]
        [EmailAddress(ErrorMessage = "Email 格式不正確")]
        public string Email { get; set; } = string.Empty;

        
        public string? FullName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
    }
}
