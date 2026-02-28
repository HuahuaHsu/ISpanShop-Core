using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using ISpanShop.Services; // 引用你的 Service 層

namespace ISpanShop.WebAPI.Hubs
{
	// 必須繼承 SignalR 的 Hub 類別
	public class ChatHub : Hub
	{
		// 宣告我們剛剛寫好的 ChatService
		private readonly IChatService _chatService;

		// 透過依賴注入 (DI) 把 Service 帶進來
		public ChatHub(IChatService chatService)
		{
			_chatService = chatService;
		}

		// 這個方法名稱 "SendPrivateMessage" 就是前端 JavaScript 要呼叫的名稱
		public async Task SendPrivateMessage(int receiverId, string content, byte type)
		{
			// 1. 取得當下發送者的 ID
			// (實務上，使用者登入後 SignalR 會把 ID 放在 Context.UserIdentifier)
			// 這裡我們先假設已經成功取得使用者的 ID
			int senderId = int.Parse(Context.UserIdentifier ?? "0");

			if (senderId == 0)
			{
				await Clients.Caller.SendAsync("ErrorMessage", "請先登入！");
				return;
			}

			try
			{
				// 2. 呼叫 Service 執行核心邏輯 (包含：過濾髒話、存入 ChatMessages 資料庫)
				await _chatService.SendMessageAsync(senderId, receiverId, content, type);

				// 3. 發送成功後，透過 SignalR 即時推播訊息！
				// 推播給接收者
				await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", senderId, content, type, DateTime.Now.ToString("HH:mm"));

				// 也推播給發送者自己 (讓自己的對話框顯示剛剛發出的訊息)
				await Clients.Caller.SendAsync("ReceiveMessage", senderId, content, type, DateTime.Now.ToString("HH:mm"));
			}
			catch (Exception ex)
			{
				// 如果存入資料庫失敗，回傳錯誤給發送者
				await Clients.Caller.SendAsync("ErrorMessage", "訊息發送失敗，請稍後再試。");
			}
		}
	}
}
