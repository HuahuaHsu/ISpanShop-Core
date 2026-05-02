using System;
using System.Collections.Generic;
using ISpanShop.Common.Enums;

namespace ISpanShop.Models.DTOs.Orders
{
    public class FrontOrderDetailDto
    {
        public long Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? ShippingFee { get; set; }
        public int? PointDiscount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? LevelDiscount { get; set; }
        public int? CouponId { get; set; }
        public string CouponTitle { get; set; }
        public decimal? PromotionDiscount { get; set; } // 新增：賣場活動促銷折抵總額
        public decimal FinalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; }
        
        public int StoreId { get; set; }
        public int SellerId { get; set; }
        public string StoreName { get; set; }
        public int StoreStatus { get; set; }
        
        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }
        public string RecipientAddress { get; set; }
        public string Note { get; set; }
        
        public List<FrontOrderItemDto> Items { get; set; } = new List<FrontOrderItemDto>();
        public bool IsReviewed { get; set; }
        public bool HasAppealed { get; set; }

        // 退貨退款資訊快照
        public FrontReturnDetailDto ReturnInfo { get; set; }
    }

    public class FrontReturnDetailDto
    {
        public string ReasonCategory { get; set; }
        public string ReasonDescription { get; set; }
        public decimal RefundAmount { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> ImageUrls { get; set; }
        public List<FrontReturnItemDto> Items { get; set; } = new List<FrontReturnItemDto>();
    }

    public class FrontReturnItemDto
    {
        public string ProductName { get; set; }
        public string VariantName { get; set; }
        public string CoverImage { get; set; }
        public decimal Price { get; set; }
        public int ReturnQuantity { get; set; }
        public List<string> PromotionTags { get; set; } = new List<string>(); // 新增：活動標籤
    }

    public class FrontOrderItemDto
    {
        public long Id { get; set; }
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public string ProductName { get; set; }
        public string VariantName { get; set; }
        public string CoverImage { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int StoreStatus { get; set; }
        public List<string> PromotionTags { get; set; } = new List<string>(); // 新增：活動標籤
    }
}
