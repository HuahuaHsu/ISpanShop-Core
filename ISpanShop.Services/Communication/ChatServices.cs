using System;
using System.Collections.Generic;
using System.Linq;
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
    Task<List<ISpanShop.Models.DTOs.Common.ChatSessionDto>> GetChatSessionsAsync(int userId);

    // 標記訊息為已讀
    Task MarkMessagesAsReadAsync(int senderId, int receiverId);
}

// 2. 實作
public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepo;
    private readonly ISensitiveWordRepository _wordRepo;

    public ChatService(IChatRepository chatRepo, ISensitiveWordRepository wordRepo)
    {
        _chatRepo = chatRepo;
        _wordRepo = wordRepo;
    }

    public async Task MarkMessagesAsReadAsync(int senderId, int receiverId)
    {
        await _chatRepo.MarkAsReadAsync(senderId, receiverId);
    }

    public async Task<List<ChatMessage>> GetChatHistoryAsync(int user1Id, int user2Id)
    {
        // 將對方傳來的訊息標記為已讀
        await _chatRepo.MarkAsReadAsync(user2Id, user1Id);
        return await _chatRepo.GetChatHistoryAsync(user1Id, user2Id);
    }

    public async Task<List<ISpanShop.Models.DTOs.Common.ChatSessionDto>> GetChatSessionsAsync(int userId)
    {
        return await _chatRepo.GetChatSessionsAsync(userId);
    }

    public async Task SendMessageAsync(int senderId, int receiverId, string content, byte type)
    {
        // 1. 取得所有敏感字庫
        var badWords = await _wordRepo.GetAllWordsAsync();

        // 2. 敏感字過濾處理
        string cleanContent = content;
        if (!string.IsNullOrEmpty(cleanContent) && badWords != null && badWords.Any())
        {
            foreach (var word in badWords)
            {
                if (cleanContent.Contains(word))
                {
                    cleanContent = cleanContent.Replace(word, new string('*', word.Length));
                }
            }
        }

        // 3. 封裝 Entity
        var message = new ChatMessage
        {
            SessionId = Guid.NewGuid(),
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = cleanContent,
            Type = type,
            IsRead = false,
            SentAt = DateTime.Now
        };

        // 4. 存入資料庫
        await _chatRepo.AddMessageAsync(message);
    }
}
