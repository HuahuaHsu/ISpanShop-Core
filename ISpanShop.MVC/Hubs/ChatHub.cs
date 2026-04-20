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
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        // 當客戶端發送訊息時呼叫此方法
        public async Task SendMessage(int receiverId, string content, byte type)
        {
            var senderIdStr = Context.UserIdentifier;
            
            _logger.LogInformation($"ChatHub: Attempting to send message from {senderIdStr} to {receiverId}");

            if (int.TryParse(senderIdStr, out int senderId))
            {
                await _chatService.SendMessageAsync(senderId, receiverId, content, type);
                await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", senderId, content, type);
                await Clients.Caller.SendAsync("ReceiveMessage", senderId, content, type);
            }
            else
            {
                _logger.LogWarning("ChatHub: Could not identify sender ID.");
            }
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"ChatHub: User connected - {Context.UserIdentifier}");
            await base.OnConnectedAsync();
        }
    }
}
