using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISpanShop.Services.Communication;
using System.Security.Claims;

namespace ISpanShop.MVC.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "FrontendJwt")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        // 取得目前的對話清單
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                var sessions = await _chatService.GetChatSessionsAsync(userId);
                return Ok(sessions);
            }
            return Unauthorized();
        }

        // 取得特定對象的對話紀錄
        [HttpGet("history/{otherUserId}")]
        public async Task<IActionResult> GetHistory(int otherUserId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                var history = await _chatService.GetChatHistoryAsync(userId, otherUserId);
                return Ok(history);
            }
            return Unauthorized();
        }
    }
}
