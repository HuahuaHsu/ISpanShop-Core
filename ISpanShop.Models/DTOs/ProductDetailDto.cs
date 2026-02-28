using System.Collections.Generic;

namespace ISpanShop.Models.DTOs
{
    /// <summary>
    /// 商品詳情 DTO - 用於商品詳情頁展示（包含完整的圖片與規格）
    /// </summary>
    public class ProductDetailDto
    {
        /// <summary>
        /// 商品 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 商品名稱
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// 商店名稱
        /// </summary>
        public string? StoreName { get; set; }

        /// <summary>
        /// 分類名稱
        /// </summary>
        public string? CategoryName { get; set; }

        /// <summary>
        /// 品牌名稱
        /// </summary>
        public string? BrandName { get; set; }

        /// <summary>
        /// 商品描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 商品狀態 (1=已上架, 2=待審核, 0=下架)
        /// </summary>
        public byte? Status { get; set; }

        /// <summary>
        /// 商品圖片 URL 列表
        /// </summary>
        public List<string> Images { get; set; } = new();

        /// <summary>
        /// 商品規格變體列表
        /// </summary>
        public List<ProductVariantDetailDto> Variants { get; set; } = new();
    }
}
