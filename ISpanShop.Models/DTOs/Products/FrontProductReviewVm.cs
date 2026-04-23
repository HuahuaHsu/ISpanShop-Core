using System;
using System.Collections.Generic;

namespace ISpanShop.Models.DTOs.Products
{
    /// <summary>
    /// 專門給前台商品詳情頁顯示用的評論資訊
    /// </summary>
    public class FrontProductReviewVm
    {
        public int Id { get; set; }
        public string UserName { get; set; } // 如 c****4
        public string UserAvatar { get; set; }
        public byte Rating { get; set; }
        public string Comment { get; set; }
        public string StoreReply { get; set; }
        public DateTime CreatedAt { get; set; }
        public string VariantName { get; set; } // 使用者購買的規格
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
