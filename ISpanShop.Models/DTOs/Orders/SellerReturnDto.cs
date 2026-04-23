using ISpanShop.Common.Enums;
using System;
using System.Collections.Generic;

namespace ISpanShop.Models.DTOs.Orders
{
    public class SellerReturnListDto
    {
        public long Id { get; set; } // OrderId
        public string OrderNumber { get; set; }
        public string BuyerName { get; set; }
        public decimal RefundAmount { get; set; }
        public string ReasonCategory { get; set; }
        public DateTime CreatedAt { get; set; } // 申請時間
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; }
    }

    public class SellerReturnDetailDto
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderCreatedAt { get; set; }
        
        // 退貨資訊
        public string ReasonCategory { get; set; }
        public string ReasonDescription { get; set; }
        public decimal RefundAmount { get; set; }
        public DateTime RequestCreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; }
        
        // 證明圖片
        public List<string> ImageUrls { get; set; } = new();

        // 買家資訊
        public string BuyerAccount { get; set; }
        
        // 商品明細 (退款通常是整筆訂單，或由明細決定金額)
        public List<SellerOrderItemDto> Items { get; set; } = new();
    }

    public class ReviewReturnRequestDto
    {
        public bool IsApproved { get; set; }
        public string Remark { get; set; }
    }
}
