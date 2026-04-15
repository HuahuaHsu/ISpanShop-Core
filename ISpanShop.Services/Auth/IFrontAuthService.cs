using ISpanShop.Models.DTOs.Auth;

namespace ISpanShop.Services.Auth
{
    public interface IFrontAuthService
    {
        Task<FrontLoginResponseDto?> LoginAsync(FrontLoginRequestDto request);
        Task<bool> RegisterAsync(FrontRegisterRequestDto request);
    }
}
