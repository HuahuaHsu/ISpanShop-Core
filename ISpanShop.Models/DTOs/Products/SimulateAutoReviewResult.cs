namespace ISpanShop.Models.DTOs.Products
{
    /// <summary>
    /// 模擬自動審核的執行結果
    /// </summary>
    public class SimulateAutoReviewResult
    {
        /// <summary>自動通過上架的商品筆數</summary>
        public int ApprovedCount { get; set; }

        /// <summary>被系統攔截（直接退回）的商品筆數</summary>
        public int RejectedCount { get; set; }

        /// <summary>系統無法確定、轉交人工複審的商品筆數</summary>
        public int ManualReviewCount { get; set; }
    }
}
