namespace ISpanShop.Models.DTOs
{
    /// <summary>
    /// 商品規格詳情 DTO - 用於商品詳情頁展示
    /// </summary>
    public class ProductVariantDetailDto
    {
        /// <summary>
        /// SKU 代碼
        /// </summary>
        public required string SkuCode { get; set; }

        /// <summary>
        /// 規格名稱
        /// </summary>
        public required string VariantName { get; set; }

        /// <summary>
        /// 價格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 庫存量
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 規格值 JSON (如 {"color":"黑","size":"M"})
        /// </summary>
        public string? SpecValueJson { get; set; }
    }
}
