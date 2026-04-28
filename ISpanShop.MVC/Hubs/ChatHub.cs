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
            _logger.LogInformation($"SendMessage called: Sender={senderIdStr}, Receiver={receiverId}, Content={content}");
            
            if (int.TryParse(senderIdStr, out int senderId))
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var chatService = scope.ServiceProvider.GetRequiredService<IChatService>();
                    var botService = scope.ServiceProvider.GetRequiredService<IBotService>();

                    // 1. 執行正規的發送流程 (存入資料庫)
                    try 
                    {
                        await chatService.SendMessageAsync(senderId, receiverId, content, type);
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, "Failed to save message to database");
                    }

                    // 2. 傳送給接收者
                    await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", senderId, content, type);
                    
                    // 3. 也傳送給發送者本人 (同步 UI)
                    await Clients.Caller.SendAsync("ReceiveMessage", senderId, content, type);

                    // --- 機器人自動回覆邏輯 ---
                    // 只要不是發送給自己，就嘗試觸發機器人
                    if (receiverId != senderId && type == 0) 
                    {
                        _ = Task.Run(async () => {
                            try {
                                await Task.Delay(1000); 
                                using (var botScope = _scopeFactory.CreateScope())
                                {
                                    var innerBotService = botScope.ServiceProvider.GetRequiredService<IBotService>();
                                    var innerChatService = botScope.ServiceProvider.GetRequiredService<IChatService>();

                                    // 嘗試獲取機器人回覆
                                    string botReply = await innerBotService.GetResponseAsync(content, senderId.ToString());
                                    
                                    if (!string.IsNullOrEmpty(botReply))
                                    {
                                        // 機器人以「接收者」的身分回覆給「發送者」
                                        await innerChatService.SendMessageAsync(receiverId, senderId, botReply, 0);
                                        await Clients.User(senderId.ToString()).SendAsync("ReceiveMessage", receiverId, botReply, 0);
                                    }
                                }
                            }
                            catch (System.Exception ex) {
                                // 機器人失敗時僅記錄 Log，不再傳送錯誤訊息給前端，確保一般對話順暢
                                _logger.LogWarning($"Bot response skipped or failed: {ex.Message}");
                            }
                        });
                    }
                }
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var userName = Context.User?.Identity?.Name 
                         ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            
            _logger.LogInformation($"ChatHub: Connection from User {userName} (ID: {userId})");

            // 建立連線時發送一個歡迎訊息，確認連線成功
            await Clients.Caller.SendAsync("ReceiveMessage", 0, $"系統：連線成功，歡迎 {userName}！(ID: {userId})", 0);

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
