using ISpanShop.Models.DTOs.Auth;

namespace ISpanShop.Services.Members
{
    public interface IAccountService
    {
        Task<(bool Success, string Message)> ChangePasswordAsync(ChangePasswordDto dto);
    }
}
