using ISpanShop.Models.DTOs.Auth;
using ISpanShop.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using ISpanShop.Services.Members;

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
            // ... (existing code)
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
                // 取得 Client IP
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
            // ... (existing code)
            try
            {
                await _authService.RegisterAsync(request);
                return Ok(new { message = "註冊成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

		/// <summary>
		/// 忘記密碼 - 申請重設
		/// </summary>
		[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
		{
			try
			{
				var (isSuccess, message) = await _accountService.ForgotPasswordAsync(dto);

				// 如果 Service 回傳 false，就丟出 BadRequest (HTTP 400)
				if (!isSuccess)
				{
					return BadRequest(new { isSuccess, message });
				}

				return Ok(new { isSuccess, message });
			}
			catch (Exception ex)
			{
				return BadRequest(new { isSuccess = false, message = ex.Message });
			}
		}

		/// <summary>
		/// 重設密碼 - 提交新密碼
		/// </summary>
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
    }
}
