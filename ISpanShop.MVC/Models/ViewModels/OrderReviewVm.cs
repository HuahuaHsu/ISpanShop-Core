using System;

namespace ISpanShop.MVC.Models.ViewModels
{
    public class OrderReviewVm
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public long OrderId { get; set; }
        public byte Rating { get; set; }
        public string Comment { get; set; }
        public string StoreReply { get; set; }
        public bool IsHidden { get; set; }
        public bool IsAutoFlagged { get; set; } // [新功能] 是否被系統自動標記為敏感內容
        public DateTime CreatedAt { get; set; }
        public System.Collections.Generic.List<string> ImageUrls { get; set; } = new System.Collections.Generic.List<string>();
        public string ProductMainImage { get; set; }
    }
}