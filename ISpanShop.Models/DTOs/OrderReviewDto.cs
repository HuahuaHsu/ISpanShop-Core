using System;

namespace ISpanShop.Models.DTOs
{
    public class OrderReviewDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public long OrderId { get; set; }
        public byte Rating { get; set; } // 1-5星
        public string Comment { get; set; }
        public string StoreReply { get; set; }
        public bool IsHidden { get; set; } // 隱藏開關
        public bool IsAutoFlagged { get; set; } // [新功能] 是否被系統自動標記為敏感內容
        public DateTime CreatedAt { get; set; }
        public System.Collections.Generic.List<string> ImageUrls { get; set; } = new System.Collections.Generic.List<string>();
        public string ProductMainImage { get; set; }
    }
}