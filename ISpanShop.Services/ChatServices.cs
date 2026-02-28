using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories; // 引用倉儲層

namespace ISpanShop.Services
{
	// 1. 介面
	public interface IChatService
	{
		// 發送訊息 (包含過濾髒話的邏輯)
		Task SendMessageAsync(int senderId, int receiverId, string content, byte type);
	}

	// 2. 實作
	public class ChatService : IChatService
	{
		// 宣告兩個唯讀的 Repository 介面
		private readonly IChatRepository _chatRepo;
		private readonly ISensitiveWordsRepository _wordRepo;

		// 透過依賴注入 (DI) 把實體傳進來
		public ChatService(IChatRepository chatRepo, ISensitiveWordsRepository wordRepo)
		{
			_chatRepo = chatRepo;
			_wordRepo = wordRepo;
		}

		public async Task SendMessageAsync(int senderId, int receiverId, string content, byte type)
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
}