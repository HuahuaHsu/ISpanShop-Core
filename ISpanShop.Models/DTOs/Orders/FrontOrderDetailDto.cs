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
        public decimal FinalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; }
        
        public string StoreName { get; set; }
        
        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }
        public string RecipientAddress { get; set; }
        public string Note { get; set; }
        
        public List<FrontOrderItemDto> Items { get; set; } = new List<FrontOrderItemDto>();
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
    }
}
