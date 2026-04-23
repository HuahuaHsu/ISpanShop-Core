using ISpanShop.Common.Helpers;
using ISpanShop.Models.DTOs.Support;
using ISpanShop.Services.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ISpanShop.MVC.Controllers.Api.Support
{
    [Authorize(AuthenticationSchemes = "FrontendJwt")]
    [Route("api/support-tickets")]
    [ApiController]
    public class SupportTicketsApiController : ControllerBase
    {
        private readonly ISupportTicketService _supportService;

        public SupportTicketsApiController(ISupportTicketService supportService)
        {
            _supportService = supportService;
        }

        /// <summary>
        /// 取得當前會員的工單列表
        /// </summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyTickets()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var tickets = await _supportService.GetByUserIdAsync(userId);
            return Ok(tickets);
        }

        /// <summary>
        /// 提交新工單
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] SupportTicketDto dto)
        {
            try 
            {
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized();
                }

                // 強制綁定為當前登入者 ID
                dto.UserId = userId;
                
                await _supportService.CreateAsync(dto);
                return Ok(new { message = "工單已成功提交" });
            }
            catch (System.Exception ex)
            {
                // 捕捉資料庫錯誤 (例如無效的 OrderId) 或其他伺服器錯誤
                return BadRequest(new { message = "提交失敗：" + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        /// <summary>
        /// 取得特定工單詳情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();

            var ticket = await _supportService.GetTicketDetailsAsync(id);
            if (ticket == null) return NotFound();

            // 確保會員只能看自己的工單
            if (ticket.UserId != userId.Value) return Forbid();

            return Ok(ticket);
        }
    }
}
