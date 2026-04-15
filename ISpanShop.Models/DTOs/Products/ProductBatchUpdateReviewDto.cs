using System.Collections.Generic;

namespace ISpanShop.Models.DTOs.Products
{
    /// <summary>
    /// 批次更新商品審核狀態 DTO
    /// </summary>
    public class ProductBatchUpdateReviewDto
    {
        public List<int> ProductIds { get; set; } = new List<int>();
        /// <summary>目標審核狀態：0=待審核, 1=通過, 2=退回</summary>
        public int TargetReviewStatus { get; set; }
        /// <summary>審核人員帳號（通過/退回時記錄）</summary>
        public string? ReviewedBy { get; set; }
    }
}
