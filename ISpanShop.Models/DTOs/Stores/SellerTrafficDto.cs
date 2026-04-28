using System.Collections.Generic;

namespace ISpanShop.Models.DTOs.Stores
{
    public class SellerTrafficDto
    {
        public TrafficSummaryDto Summary { get; set; }
        public List<TopProductTrafficDto> TopProducts { get; set; }
        public List<CategoryTrafficDto> CategoryData { get; set; }
    }

    public class TrafficSummaryDto
    {
        public int TotalViews { get; set; }
        public decimal TopItemsTrafficShare { get; set; } // 前三名流量佔比 (%)
        public decimal AvgConversionRate { get; set; }    // 全店平均轉換率 (%)
    }

    public class TopProductTrafficDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int ViewCount { get; set; }
        public int TotalSales { get; set; }
        public decimal ConversionRate { get; set; } // 轉換率 (%)
    }

    public class CategoryTrafficDto
    {
        public string CategoryName { get; set; }
        public int ViewCount { get; set; }
        public decimal Percentage { get; set; }
    }
}
