using System;

namespace ISpanShop.Models.DTOs
{
    /// <summary>
    /// 商品審核 DTO - Service 層回傳的資料轉移物件
    /// </summary>
    public class ProductReviewDto
    {
        /// <summary>
        /// 商品 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 店鋪 ID
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// 分類名稱
        /// </summary>
        public required string CategoryName { get; set; }

        /// <summary>
        /// 品牌名稱
        /// </summary>
        public string? BrandName { get; set; }

        /// <summary>
        /// 商品名稱
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// 商品狀態 (1=已上架, 2=待審核, 其他值)
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime? CreatedAt { get; set; }
    }
}
