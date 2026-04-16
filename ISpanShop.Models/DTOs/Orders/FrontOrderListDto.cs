using System;
using System.Collections.Generic;
using ISpanShop.Common.Enums;

namespace ISpanShop.Models.DTOs.Orders
{
    public class FrontOrderListDto
    {
        public long Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal FinalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; }
        public string StoreName { get; set; }
        
        // 用於列表顯示的第一個商品資訊
        public string FirstProductName { get; set; }
        public string FirstProductImage { get; set; }
        public int TotalItemCount { get; set; }
    }
}
