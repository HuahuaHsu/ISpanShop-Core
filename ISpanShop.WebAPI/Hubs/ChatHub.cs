using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using ISpanShop.Services;

namespace ISpanShop.WebAPI.Hubs
{
	public class ChatHub : Hub
	{
		private readonly IChatService _chatService;

		public ChatHub(IChatService chatService)
		{
			_chatService = chatService;
		}

		// 整合後的發送方法：接收前端傳來的 myId
		public async Task SendPrivateMessage(int myId, int receiverId, string content, byte type)
		{
			// 基本檢查：確保 ID 不是 0
			if (myId == 0)
			{
				await Clients.Caller.SendAsync("ErrorMessage", "請輸入有效的發送者 ID！");
				return;
			}

			try
			{
				// 1. 執行存檔：這會將資料寫入 dbo.ChatMessages
				// 對應你資料表的欄位：SenderId = myId, ReceiverId = receiverId
				await _chatService.SendMessageAsync(myId, receiverId, content, type);

				// 2. 即時推播給雙方
				string timeString = DateTime.Now.ToString("HH:mm");

				// 推播給接收者
				await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", myId, content, type, timeString);

				// 推播給發送者 (自己)
				await Clients.Caller.SendAsync("ReceiveMessage", myId, content, type, timeString);
			}
			catch (Exception ex)
			{
				// 抓取詳細錯誤 (例如 Foreign Key 衝突) 並回傳給網頁
				string realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
				await Clients.Caller.SendAsync("ErrorMessage", $"資料庫寫入失敗：{realError}");
			}
		}
	}
}