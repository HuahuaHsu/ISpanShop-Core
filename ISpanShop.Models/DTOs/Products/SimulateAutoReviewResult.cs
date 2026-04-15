namespace ISpanShop.Models.DTOs.Products
{
    /// <summary>
    /// 單筆商品的自動審核結果
    /// </summary>
    public class AutoReviewItemResult
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        /// <summary>"Approved" | "Rejected" | "Uncertain"</summary>
        public string Outcome { get; set; } = string.Empty;

        /// <summary>比對到的敏感字清單</summary>
        public List<string> MatchedWords { get; set; } = new();
    }

    /// <summary>
    /// 模擬自動審核的彙總結果
    /// </summary>
    public class SimulateAutoReviewResult
    {
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int ManualReviewCount { get; set; }

        /// <summary>每筆商品的詳細審核結果（含比對到的敏感字）</summary>
        public List<AutoReviewItemResult> Items { get; set; } = new();
    }
}
