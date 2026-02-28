namespace ISpanShop.Models.DTOs
{
    /// <summary>
    /// 商品規格變體新增用 DTO - 用於接收規格變體資訊
    /// </summary>
    public class ProductVariantCreateDto
    {
        /// <summary>
        /// SKU 代碼
        /// </summary>
        public string? SkuCode { get; set; }

        /// <summary>
        /// 規格名稱
        /// </summary>
        public required string VariantName { get; set; }

        /// <summary>
        /// 規格值 JSON - 儲存規格屬性和對應值
        /// </summary>
        public required string SpecValueJson { get; set; }

        /// <summary>
        /// 價格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 庫存數量
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 安全庫存 - 低於此數量時需要補貨
        /// </summary>
        public int SafetyStock { get; set; }
    }
}
