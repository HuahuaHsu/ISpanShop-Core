using System;

namespace ISpanShop.Models.DTOs.Common
{
    public class ChatSessionDto
    {
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime? SentAt { get; set; }
        public int UnreadCount { get; set; }
    }
}
