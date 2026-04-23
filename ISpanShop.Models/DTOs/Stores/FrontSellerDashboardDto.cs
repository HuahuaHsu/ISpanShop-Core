using System;
using System.Collections.Generic;

namespace ISpanShop.Models.DTOs.Stores
{
    public class FrontSellerDashboardDto
    {
        public SellerKpiDto Kpis { get; set; }
        public ApexChartDataDto SalesTrend { get; set; }
        public List<TopProductSalesDto> TopProducts { get; set; }
        public List<RecentOrderDto> RecentOrders { get; set; }
    }

    public class RecentOrderDto
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string BuyerName { get; set; }
        public string ProductName { get; set; } // 顯示第一件商品或摘要
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string CreatedAt { get; set; }
    }

    public class SellerKpiDto
    {
        public decimal TotalRevenue { get; set; }      // 總累積營收
        public decimal RevenueLast7Days { get; set; }  // 近 7 天營收
        public string RevenueGrowthRate { get; set; }  // 營收較上期成長率
        public string RevenueGrowthType { get; set; }  // 成長類型: "up", "down", "neutral"
        public int TotalOrders { get; set; }           // 總訂單數
        public int OrdersLast7Days { get; set; }       // 近 7 天訂單數
        public string OrdersGrowthRate { get; set; }   // 訂單較上期成長率
        public string OrdersGrowthType { get; set; }   // 成長類型: "up", "down", "neutral"
        public int PendingOrders { get; set; }         // 待出貨訂單
        public int PendingRefundCount { get; set; }    // 待審核退貨數
        public int TotalProducts { get; set; }         // 在架商品數
        public int LowStockCount { get; set; }         // 庫存低於預警商品數
    }
}
