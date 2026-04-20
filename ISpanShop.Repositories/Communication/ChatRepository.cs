using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ISpanShop.Models.EfModels;

namespace ISpanShop.Repositories.Communication
{
    public interface IChatRepository
    {
        Task<int> GetUnreadCountAsync(int receiverId);
        Task MarkAsReadAsync(int senderId, int receiverId);
        Task AddMessageAsync(ChatMessage message);
        Task<List<ChatMessage>> GetChatHistoryAsync(int user1Id, int user2Id);
        Task<List<ISpanShop.Models.DTOs.Common.ChatSessionDto>> GetChatSessionsAsync(int userId);
    }

    public class ChatRepository : IChatRepository
    {
        private readonly ISpanShopDBContext _context;

        public ChatRepository(ISpanShopDBContext context)
        {
            _context = context;
        }

        public async Task<int> GetUnreadCountAsync(int receiverId)
        {
            return await _context.ChatMessages
                .Where(m => m.ReceiverId == receiverId && m.IsRead == false)
                .CountAsync();
        }

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

        public async Task AddMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ChatMessage>> GetChatHistoryAsync(int user1Id, int user2Id)
        {
            return await _context.ChatMessages
                .Where(m => (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                            (m.SenderId == user2Id && m.ReceiverId == user1Id))
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<List<ISpanShop.Models.DTOs.Common.ChatSessionDto>> GetChatSessionsAsync(int userId)
        {
            // 找出所有與該使用者有關的訊息
            var allMessages = await _context.ChatMessages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            // 取得所有對話對象的 ID
            var otherUserIds = allMessages
                .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToList();

            // 一次撈出這些對象的姓名資訊 (優先顯示商店名稱，若無則顯示姓名)
            var userInfos = await _context.Users
                .Include(u => u.MemberProfile)
                .Include(u => u.Products)
                .Where(u => otherUserIds.Contains(u.Id))
                .Select(u => new { 
                    u.Id, 
                    DisplayName = _context.Stores.Where(s => s.UserId == u.Id).Select(s => s.StoreName).FirstOrDefault() 
                                 ?? u.MemberProfile.FullName 
                                 ?? u.Account 
                })
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? "未知用戶");

            // 依對象分組，取得最後一則訊息與未讀數
            var sessions = allMessages
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => {
                    var lastMsg = g.First();
                    // 根據訊息類型顯示對應的預覽文字
                    string displayMsg = lastMsg.Type switch
                    {
                        1 => "[圖片]",
                        2 => "[影片]",
                        3 => "[檔案]",
                        _ => lastMsg.Content
                    };
                    
                    return new ISpanShop.Models.DTOs.Common.ChatSessionDto
                    {
                        OtherUserId = g.Key,
                        OtherUserName = userInfos.ContainsKey(g.Key) ? userInfos[g.Key] : $"用戶 {g.Key}",
                        LastMessage = displayMsg,
                        SentAt = lastMsg.SentAt,
                        UnreadCount = g.Count(m => m.ReceiverId == userId && m.IsRead == false)
                    };
                })
                .ToList();

            return sessions;
        }
    }
}
