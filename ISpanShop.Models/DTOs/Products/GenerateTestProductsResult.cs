namespace ISpanShop.Models.DTOs.Products
{
    /// <summary>
    /// 生成測試商品的執行結果
    /// </summary>
    public class GenerateTestProductsResult
    {
        public int TotalCount      { get; set; }
        public int CleanCount      { get; set; }
        public int HighRiskCount   { get; set; }
        public int BorderlineCount { get; set; }
        public IEnumerable<ProductReviewDto> CreatedProducts { get; set; } = new List<ProductReviewDto>();
    }
}
