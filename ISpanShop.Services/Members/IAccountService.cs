using ISpanShop.Models.DTOs.Auth;

namespace ISpanShop.Services.Members
{
    public interface IAccountService
    {
        Task<(bool IsSuccess, string Message)> ChangePasswordAsync(ChangePasswordDto dto);
        Task<(bool IsSuccess, string Message)> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<(bool IsSuccess, string Message)> ResetPasswordAsync(ResetPasswordDto dto);
    }
}
