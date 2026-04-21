using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ISpanShop.MVC.Hubs
{
    public class NameUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            // 強制 SignalR 使用 JWT 中的 NameIdentifier (memberId) 或 sub 作為 User ID
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? connection.User?.FindFirst("sub")?.Value;
        }
    }
}
