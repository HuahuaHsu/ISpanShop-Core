using ISpanShop.Common.Helpers;
using ISpanShop.Models.DTOs.Auth;
using ISpanShop.Services.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Controllers.Api
{
    [ApiController]
    [Route("api/front/member")]
    [Authorize(AuthenticationSchemes = "FrontendJwt")]
    public class FrontMemberController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public FrontMemberController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// 變更密碼 (需透過 JWT 驗證)
        /// </summary>
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                // 從 Token 中安全取得 UserId，並覆寫到 dto 中
                var currentUserId = User.GetUserId();
                if (currentUserId == null)
                    return Unauthorized(new { isSuccess = false, message = "無法取得目前使用者 ID" });

                dto.UserId = currentUserId.Value;

                var (isSuccess, message) = await _accountService.ChangePasswordAsync(dto);
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
