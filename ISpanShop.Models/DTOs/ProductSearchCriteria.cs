using System;

namespace ISpanShop.Models.DTOs
{
    /// <summary>
    /// 商品搜尋條件 - 用於分頁與多維度篩選
    /// </summary>
    public class ProductSearchCriteria
    {
        /// <summary>
        /// 主分類 ID（若有值，撈該主分類下所有子分類的商品）
        /// </summary>
        public int? ParentCategoryId { get; set; }

        /// <summary>
        /// 子分類 ID（若有值，優先以此篩選）
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// 關鍵字搜尋（搜尋商品名稱或描述）
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 品牌 ID
        /// </summary>
        public int? BrandId { get; set; }

        /// <summary>
        /// 商家 ID
        /// </summary>
        public int? StoreId { get; set; }

        /// <summary>
        /// 商品狀態（例如 1:已上架, 0:未上架/下架）
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 建檔日期起（含當天）
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 建檔日期迄（含當天整天）
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 頁碼（從 1 開始）
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
