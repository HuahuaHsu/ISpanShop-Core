namespace ISpanShop.Models.DTOs.Products
{
    /// <summary>商品活動資訊（列表用）</summary>
    public class ProductPromotionInfoDto
    {
        public int      PromotionId     { get; set; }
        public string   PromotionName   { get; set; } = string.Empty;
        public string   Type            { get; set; } = string.Empty;
        public string   TypeLabel       { get; set; } = string.Empty;
        public decimal? DiscountPrice   { get; set; }
        public int?     DiscountPercent { get; set; }
        public decimal  OriginalPrice   { get; set; }
        public DateTime EndDate         { get; set; }
        public PromotionRuleInfoDto? Rule { get; set; }
    }

    /// <summary>活動規則資訊</summary>
    public class PromotionRuleInfoDto
    {
        public decimal Threshold     { get; set; }
        public int     DiscountType  { get; set; }
        public decimal DiscountValue { get; set; }
    }
}
