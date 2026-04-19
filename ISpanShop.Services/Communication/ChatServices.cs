using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Communication;
using ISpanShop.Repositories.ContentModeration;

namespace ISpanShop.Services.Communication;

// 1. 介面
public interface IChatService
{
	// 發送訊息 (包含過濾髒話的邏輯)
	Task SendMessageAsync(int senderId, int receiverId, string content, byte type);

    // 取得歷史紀錄
    Task<List<ChatMessage>> GetChatHistoryAsync(int user1Id, int user2Id);

    // 取得對話清單
    Task<List<dynamic>> GetChatSessionsAsync(int userId);
}

// 2. 實作
public class ChatService : IChatService
{
		// ... (existing constructor)
		private readonly IChatRepository _chatRepo;
		private readonly ISensitiveWordRepository _wordRepo;

		public ChatService(IChatRepository chatRepo, ISensitiveWordRepository wordRepo)
		{
			_chatRepo = chatRepo;
			_wordRepo = wordRepo;
		}

		// ... (existing SendMessageAsync)

        public async Task<List<ChatMessage>> GetChatHistoryAsync(int user1Id, int user2Id)
        {
            // 在載入紀錄時，順便將訊息標記為已讀
            await _chatRepo.MarkAsReadAsync(user2Id, user1Id);
            return await _chatRepo.GetChatHistoryAsync(user1Id, user2Id);
        }

        public async Task<List<dynamic>> GetChatSessionsAsync(int userId)
        {
            return await _chatRepo.GetChatSessionsAsync(userId);
        }

		public async Task SendMessageAsync(int senderId, int receiverId, string content, byte type)
// ... rest of file
		{
			// --- 商業邏輯區塊開始 ---

			// 1. 取得所有敏感字庫
			var badWords = await _wordRepo.GetAllWordsAsync();

			// 2. 敏感字過濾處理
			string cleanContent = content;
			if (!string.IsNullOrEmpty(cleanContent) && badWords.Any())
			{
				foreach (var word in badWords)
				{
					if (cleanContent.Contains(word))
					{
						// 將髒話替換成等長的星號，例如 "王八蛋" 變成 "***"
						cleanContent = cleanContent.Replace(word, new string('*', word.Length));
					}
				}
			}

			// 3. 將清理過的資料封裝成 Entity Model
			var message = new ChatMessage
			{
				SessionId = Guid.NewGuid(), // 實務上這通常由前端傳入同一組對話的 ID
				SenderId = senderId,
				ReceiverId = receiverId,
				Content = cleanContent,      // 存入的是過濾後的乾淨內容
				Type = type,
				IsRead = false,
				SentAt = DateTime.Now
			};

			// 4. 呼叫 ChatRepository 存入資料庫
			await _chatRepo.AddMessageAsync(message);

			// --- 商業邏輯區塊結束 ---
		}
	}