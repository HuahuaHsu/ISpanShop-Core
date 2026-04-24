using System;
using ISpanShop.Common.Enums;

namespace ISpanShop.Models.DTOs.Stores
{
    public class SellerOrderListDto
    {
        public long Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal FinalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; }
        public string BuyerName { get; set; }
        public int BuyerId { get; set; }
        public string RecipientName { get; set; }
        
        // 用於列表顯示的第一個商品資訊
        public string FirstProductName { get; set; }
        public string FirstProductImage { get; set; }
        public int TotalItemCount { get; set; }
    }
}
