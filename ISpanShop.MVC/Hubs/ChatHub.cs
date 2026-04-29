using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ISpanShop.Services.Communication;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ISpanShop.MVC.Hubs
{
    [Authorize(AuthenticationSchemes = "FrontendJwt")]
    public class ChatHub : Hub
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ChatHub> _logger;
        
        // 暫時記錄 fuen50 的 ID，用於模擬賣家
        private static string _simulatedSellerId = null;

        public ChatHub(IServiceScopeFactory scopeFactory, ILogger<ChatHub> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        // 當客戶端發送訊息時呼叫此方法
        public async Task SendMessage(int receiverId, string content, byte type)
        {
            var senderIdStr = Context.UserIdentifier;
            
            if (int.TryParse(senderIdStr, out int senderId))
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var chatService = scope.ServiceProvider.GetRequiredService<IChatService>();

                    // --- 核心模擬邏輯：確保訊息能正確傳遞 ---
                    int finalReceiverId = receiverId;
                    
                    if (!string.IsNullOrEmpty(_simulatedSellerId) && int.TryParse(_simulatedSellerId, out int sellerId))
                    {
                        if (senderId != sellerId)
                        {
                            finalReceiverId = sellerId;
                        }
                    }

                    // 1. 執行正規的發送流程 (存入資料庫)
                    try 
                    {
                        await chatService.SendMessageAsync(senderId, finalReceiverId, content, type);
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, "Failed to save message to database");
                    }

                    // 2. 傳送給接收者
                    await Clients.User(finalReceiverId.ToString()).SendAsync("ReceiveMessage", senderId, content, type);
                    
                    // 3. 也傳送給發送者本人 (同步 UI)
                    await Clients.Caller.SendAsync("ReceiveMessage", senderId, content, type);
                }
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var userName = Context.User?.Identity?.Name 
                         ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            
            _logger.LogInformation($"ChatHub: Connection from User {userName} (ID: {userId})");

            // 如果帳號包含 fuen50，就鎖定為賣家
            if (!string.IsNullOrEmpty(userName) && userName.ToLower().Contains("fuen50"))
            {
                _simulatedSellerId = userId;
                _logger.LogInformation($"[Simulation] Seller Identified: {userName} (ID: {userId})");
            }
            
            await base.OnConnectedAsync();
        }
    }
}
