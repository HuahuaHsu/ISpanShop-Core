using ISpanShop.Common.Enums;
using System;
using System.Collections.Generic;

namespace ISpanShop.Models.DTOs.Orders
{
    public class SellerOrderDetailDto
    {
        public long Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; }
        
        // 買家資訊
        public int UserId { get; set; }
        public string BuyerAccount { get; set; }
        public string BuyerName { get; set; } // FullName
        public string BuyerPhone { get; set; }
        public string BuyerEmail { get; set; }

        // 收件資訊
        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }
        public string RecipientAddress { get; set; }
        public string Note { get; set; }

        // 金額資訊
        public decimal TotalAmount { get; set; }
        public decimal? ShippingFee { get; set; }
        public decimal? DiscountAmount { get; set; }
        public int? PointDiscount { get; set; }
        public decimal FinalAmount { get; set; }

        // 商品明細
        public List<SellerOrderItemDto> Items { get; set; } = new();
    }

    public class SellerOrderItemDto
    {
        public long Id { get; set; }
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public string ProductName { get; set; }
        public string VariantName { get; set; }
        public string SkuCode { get; set; }
        public string CoverImage { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => Price * Quantity;
    }
}
