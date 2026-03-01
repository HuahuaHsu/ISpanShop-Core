namespace ISpanShop.Models.DTOs
{
    /// <summary>
    /// 商品搜尋條件 - 用於分頁與分類篩選
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
        /// 頁碼（從 1 開始）
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
