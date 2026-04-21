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
                    var botService = scope.ServiceProvider.GetRequiredService<IBotService>();

                    // --- 核心模擬邏輯：將買家訊息導向 fuen50 ---
                    int finalReceiverId = receiverId;
                    bool isBuyerSending = false;
                    
                    if (!string.IsNullOrEmpty(_simulatedSellerId) && int.TryParse(_simulatedSellerId, out int sellerId))
                    {
                        if (senderId != sellerId)
                        {
                            finalReceiverId = sellerId;
                            isBuyerSending = true;
                        }
                    }

                    // 1. 執行正規的發送流程 (存入資料庫)
                    await chatService.SendMessageAsync(senderId, finalReceiverId, content, type);

                    // 2. 傳送給接收者
                    await Clients.User(finalReceiverId.ToString()).SendAsync("ReceiveMessage", senderId, content, type);
                    
                    // 3. 也傳送給發送者本人
                    await Clients.Caller.SendAsync("ReceiveMessage", senderId, content, type);

                    // --- 機器人自動回覆邏輯 ---
                    if (isBuyerSending && type == 0) 
                    {
                        // 使用全新的 Scope 處理非同步回覆，避免 Service 被回收
                        _ = Task.Run(async () => {
                            try {
                                await Task.Delay(1500);
                                using (var botScope = _scopeFactory.CreateScope())
                                {
                                    var innerChatService = botScope.ServiceProvider.GetRequiredService<IChatService>();
                                    var innerBotService = botScope.ServiceProvider.GetRequiredService<IBotService>();

                                    string botReply = await innerBotService.GetResponseAsync(content);
                                    
                                    // 機器人以賣家 (finalReceiverId) 的身分回覆
                                    await innerChatService.SendMessageAsync(finalReceiverId, senderId, botReply, 0);
                                    
                                    // 透過 SignalR 發送
                                    await Clients.User(senderId.ToString()).SendAsync("ReceiveMessage", finalReceiverId, botReply, 0);
                                    await Clients.User(finalReceiverId.ToString()).SendAsync("ReceiveMessage", finalReceiverId, botReply, 0);
                                }
                            }
                            catch (System.Exception ex) {
                                _logger.LogError(ex, "Bot Auto-Reply Error");
                            }
                        });
                    }
                }
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            // 檢查所有 Claims 以確保抓到帳號名稱
            var userName = Context.User?.Identity?.Name 
                         ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            
            _logger.LogInformation($"ChatHub: Connection from User {userName} (ID: {userId})");

            // 如果帳號包含 fuen50，就鎖定為賣家
            if (!string.IsNullOrEmpty(userName) && userName.ToLower().Contains("fuen50"))
            {
                _simulatedSellerId = userId;
                _logger.LogInformation($"[Simulation] Seller Identified: {userName} (ID: {userId})");
            }
            else if (string.IsNullOrEmpty(_simulatedSellerId) && !string.IsNullOrEmpty(userName) && !userName.Contains("fuen49"))
            {
                _simulatedSellerId = userId;
            }

            await base.OnConnectedAsync();
        }
    }
}
