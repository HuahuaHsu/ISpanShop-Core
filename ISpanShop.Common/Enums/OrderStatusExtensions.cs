using System;

namespace ISpanShop.Common.Enums
{
    public static class OrderStatusExtensions
    {
        public static string GetDisplayName(this OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "待付款",
                OrderStatus.Processing => "待出貨",
                OrderStatus.Shipped => "運送中",
                OrderStatus.Completed => "已完成",
                OrderStatus.Cancelled => "已取消",
                OrderStatus.Returning => "退貨/款中",
                OrderStatus.Refunded => "已退款",
                _ => status.ToString()
            };
        }
    }
}
