using ISpanShop.Models.DTOs.Auth;
using ISpanShop.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using ISpanShop.Services.Members;
using ISpanShop.Common.Helpers;

namespace ISpanShop.MVC.Controllers.Api
{
    [ApiController]
    [Route("api/front/auth")]
    public class FrontAuthController : ControllerBase
    {
        private readonly IFrontAuthService _authService;
        private readonly IAccountService _accountService;

        public FrontAuthController(IFrontAuthService authService, IAccountService accountService)
        {
            _authService = authService;
            _accountService = accountService;
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = "FrontendJwt")]
        public IActionResult GetMe()
        {
            var userId = User.GetUserId();
            var account = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst("RoleId")?.Value;

            return Ok(new {
                userId = userId,
                account = account,
                role = role,
                authType = "JWT Bearer"
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] FrontLoginRequestDto request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var response = await _authService.LoginAsync(request, ipAddress);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] FrontRegisterRequestDto request)
        {
            try
            {
                await _authService.RegisterAsync(request);
                return Ok(new { message = "註冊成功，請至信箱完成 Email 驗證" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromQuery] string code)
        {
            try
            {
                var (isSuccess, message) = await _authService.VerifyEmailAsync(code);
                if (!isSuccess) return BadRequest(new { isSuccess, message });

                return Ok(new { isSuccess, message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

		[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
		{
			try
			{
				var (isSuccess, message) = await _accountService.ForgotPasswordAsync(dto);
				if (!isSuccess) return BadRequest(new { isSuccess, message });

				return Ok(new { isSuccess, message });
			}
			catch (Exception ex)
			{
				return BadRequest(new { isSuccess = false, message = ex.Message });
			}
		}

		[HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                var (isSuccess, message) = await _accountService.ResetPasswordAsync(dto);
                if (!isSuccess) return BadRequest(new { isSuccess, message });

                return Ok(new { isSuccess, message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost("oauth/google")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] OAuthExchangeDto dto)
        {
            try
            {
                var result = await _authService.OAuthLoginAsync(dto.Code, dto.RedirectUri);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("oauth/bind-callback")]
        [Authorize(AuthenticationSchemes = "FrontendJwt")]
        public async Task<IActionResult> BindOAuth([FromBody] OAuthExchangeDto dto)
        {
            try
            {
                var userId = User.GetUserId();
                if (userId == null) return Unauthorized(new { message = "驗證失效，請重新登入" });

                var success = await _authService.BindOAuthAsync(userId.Value, dto.Code, dto.RedirectUri);
                return Ok(new { success, message = "綁定成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("oauth/merge")]
        [AllowAnonymous]
        public async Task<IActionResult> MergeOAuth([FromBody] OAuthMergeDto dto)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var response = await _authService.MergeOAuthAccountAsync(dto, ipAddress);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("oauth/unbind")]
        [Authorize(AuthenticationSchemes = "FrontendJwt")]
        public async Task<IActionResult> UnbindOAuth()
        {
            try
            {
                var userId = User.GetUserId();
                if (userId == null) return Unauthorized(new { message = "驗證失效，請重新登入" });

                var success = await _authService.UnbindOAuthAsync(userId.Value);
                return Ok(new { success, message = "已解除綁定" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

       
    }
}
