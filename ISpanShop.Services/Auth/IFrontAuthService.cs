using ISpanShop.Models.DTOs.Auth;

namespace ISpanShop.Services.Auth
{
    public interface IFrontAuthService
    {
        Task<FrontLoginResponseDto?> LoginAsync(FrontLoginRequestDto request, string ipAddress);
        Task<bool> RegisterAsync(FrontRegisterRequestDto request);
    }
}
