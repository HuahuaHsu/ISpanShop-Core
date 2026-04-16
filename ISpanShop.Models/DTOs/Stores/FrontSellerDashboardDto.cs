using System;
using System.Collections.Generic;

namespace ISpanShop.Models.DTOs.Stores
{
    public class FrontSellerDashboardDto
    {
        public SellerKpiDto Kpis { get; set; }
        public ApexChartDataDto SalesTrend { get; set; }
        public List<TopProductSalesDto> TopProducts { get; set; }
    }

    public class SellerKpiDto
    {
        public decimal TotalRevenue { get; set; }      // 總營收
        public int TotalOrders { get; set; }           // 總訂單數
        public int PendingOrders { get; set; }         // 待出貨訂單
        public int TotalProducts { get; set; }         // 在架商品數
        public int LowStockCount { get; set; }         // 庫存低於預警商品數
    }
}
