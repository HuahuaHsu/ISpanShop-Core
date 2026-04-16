using ISpanShop.Models.DTOs.Auth;
using ISpanShop.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;
using System.Threading.Tasks;

namespace ISpanShop.MVC.Controllers.Api
{
    [ApiController]
    [Route("api/front/auth")]
    public class FrontAuthController : ControllerBase
    {
        private readonly IFrontAuthService _authService;

        public FrontAuthController(IFrontAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = "FrontendJwt")]
        public IActionResult GetMe()
        {
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
                var response = await _authService.LoginAsync(request);
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
                return Ok(new { message = "註冊成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
