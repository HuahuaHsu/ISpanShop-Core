using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISpanShop.Services.Communication;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ISpanShop.MVC.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "FrontendJwt")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IWebHostEnvironment _env;

        public ChatController(IChatService chatService, IWebHostEnvironment env)
        {
            _chatService = chatService;
            _env = env;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("檔案為空");

            // 1. 建立上傳目錄 wwwroot/uploads/chat
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "chat");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            // 2. 產生唯一檔名，避免重複
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadDir, fileName);

            // 3. 儲存檔案到硬碟
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 4. 回傳網址 (例如 /uploads/chat/abc.mp4)
            var fileUrl = $"/uploads/chat/{fileName}";
            return Ok(new { url = fileUrl });
        }

        // 取得目前的對話清單
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value;
            
            if (int.TryParse(userIdStr, out int userId))
            {
                var sessions = await _chatService.GetChatSessionsAsync(userId);
                return Ok(sessions);
            }
            
            return Unauthorized(new { message = "無法識別使用者身份" });
        }

        // 取得特定對象的對話紀錄
        [HttpGet("history/{otherUserId}")]
        public async Task<IActionResult> GetHistory(int otherUserId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value;
            
            if (int.TryParse(userIdStr, out int userId))
            {
                var history = await _chatService.GetChatHistoryAsync(userId, otherUserId);
                return Ok(history);
            }
            
            return Unauthorized(new { message = "無法識別使用者身份" });
        }
    }
}
