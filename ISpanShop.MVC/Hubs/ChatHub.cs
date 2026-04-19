using Microsoft.AspNetCore.SignalR;
using ISpanShop.Services.Communication;
using System.Threading.Tasks;

namespace ISpanShop.MVC.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        // 當客戶端發送訊息時呼叫此方法
        public async Task SendMessage(int receiverId, string content, byte type)
        {
            // 1. 從 Context 取得發送者 ID (假設已登入且 Claim 中有 NameIdentifier)
            var senderIdStr = Context.UserIdentifier;
            if (int.TryParse(senderIdStr, out int senderId))
            {
                // 2. 存入資料庫 (透過 Service 處理髒話過濾)
                await _chatService.SendMessageAsync(senderId, receiverId, content, type);

                // 3. 即時推播給接收者與發送者自己 (更新畫面)
                // 蝦皮聊聊通常是推送到特定的 User
                await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", senderId, content, type);
                await Clients.Caller.SendAsync("ReceiveMessage", senderId, content, type);
            }
        }

        // 使用者連線時的操作 (例如加入特定群組或紀錄上線狀態)
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}
