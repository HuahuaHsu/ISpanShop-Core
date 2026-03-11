using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ISpanShop.Models.EfModels; // 引用你生成的實體模型

namespace ISpanShop.Repositories
{
	// 1. 定義介面 (Interface)
	public interface IChatRepository
	{
		// 取得使用者的未讀訊息總數 (用於導覽列紅點)
		Task<int> GetUnreadCountAsync(int receiverId);

		// 將特定對象傳來的訊息標記為「已讀」
		Task MarkAsReadAsync(int senderId, int receiverId);

		// 儲存新訊息
		Task AddMessageAsync(ChatMessage message);

		// 取得買賣家雙方的歷史聊天紀錄
		Task<List<ChatMessage>> GetChatHistoryAsync(int user1Id, int user2Id);
	}

	// 2. 實作介面
	public class ChatRepository : IChatRepository
	{
		// 這裡替換成你實際的 DbContext 名稱 (通常叫 ISpanShopContext 或 AppDbContext)
		private readonly ISpanShopDBContext _context;

		public ChatRepository(ISpanShopDBContext context)
		{
			_context = context;
		}

		// 實作：計算未讀數量
		public async Task<int> GetUnreadCountAsync(int receiverId)
		{
			// 利用 LINQ 去資料庫找：接收者是我，且 IsRead 為 false (0) 的數量
			return await _context.ChatMessages
				.Where(m => m.ReceiverId == receiverId && m.IsRead == false)
				.CountAsync();
		}

		// 實作：標記為已讀
		public async Task MarkAsReadAsync(int senderId, int receiverId)
		{
			var unreadMessages = await _context.ChatMessages
				.Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && m.IsRead == false)
				.ToListAsync();

			if (unreadMessages.Any())
			{
				foreach (var msg in unreadMessages)
				{
					msg.IsRead = true;
				}
				await _context.SaveChangesAsync();
			}
		}

		// 實作：儲存新訊息
		public async Task AddMessageAsync(ChatMessage message)
		{
			_context.ChatMessages.Add(message);
			await _context.SaveChangesAsync();
		}

		// 實作：取得對話紀錄 (依時間排序)
		public async Task<List<ChatMessage>> GetChatHistoryAsync(int user1Id, int user2Id)
		{
			return await _context.ChatMessages
				.Where(m => (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
							(m.SenderId == user2Id && m.ReceiverId == user1Id))
				.OrderBy(m => m.SentAt)
				.ToListAsync();
		}
	}
}