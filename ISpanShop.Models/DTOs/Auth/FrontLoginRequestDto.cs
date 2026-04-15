using System.ComponentModel.DataAnnotations;

namespace ISpanShop.Models.DTOs.Auth
{
    public class FrontLoginRequestDto
    {
        [Required(ErrorMessage = "帳號為必填")]
        public string Account { get; set; } = string.Empty;

        [Required(ErrorMessage = "密碼為必填")]
        public string Password { get; set; } = string.Empty;
    }
}
