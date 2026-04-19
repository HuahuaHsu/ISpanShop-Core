using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ISpanShop.Models.EfModels; // 引用你生成的實體模型

namespace ISpanShop.Repositories.Communication
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

        // 取得聯絡人列表 (包含最後一則訊息)
        Task<List<dynamic>> GetChatSessionsAsync(int userId);
	}

	// 2. 實作介面
	public class ChatRepository : IChatRepository
	{
		// ... (existing constructor)
		private readonly ISpanShopDBContext _context;

		public ChatRepository(ISpanShopDBContext context)
		{
			_context = context;
		}

		// ... (existing methods)

		// 實作：取得聯絡人列表
		public async Task<List<dynamic>> GetChatSessionsAsync(int userId)
		{
            // 找出所有與該使用者有關的訊息，並依對象分組
            var messages = await _context.ChatMessages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            var sessions = messages
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => new
                {
                    OtherUserId = g.Key,
                    LastMessage = g.First().Content,
                    SentAt = g.First().SentAt,
                    UnreadCount = g.Count(m => m.ReceiverId == userId && m.IsRead == false)
                })
                .ToList<dynamic>();

            return sessions;
		}
	}
}