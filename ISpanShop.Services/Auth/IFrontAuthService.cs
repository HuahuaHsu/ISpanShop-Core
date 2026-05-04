using ISpanShop.Models.DTOs.Auth;

namespace ISpanShop.Services.Auth
{
    public interface IFrontAuthService
    {
        Task<FrontLoginResponseDto?> LoginAsync(FrontLoginRequestDto request, string ipAddress);
        Task<bool> RegisterAsync(FrontRegisterRequestDto request);
        Task<(bool IsSuccess, string Message)> VerifyEmailAsync(string confirmCode);
        Task<OAuthResultDto> OAuthLoginAsync(string code, string redirectUri);
        Task<FrontLoginResponseDto?> MergeOAuthAccountAsync(OAuthMergeDto dto, string ipAddress);
        Task<bool> BindOAuthAsync(int userId, string code, string redirectUri);
        Task<bool> UnbindOAuthAsync(int userId);
    }
}
