using System;

namespace ISpanShop.Models.DTOs.Products
{
    /// <summary>
    /// 商品搜尋條件 - 用於分頁與多維度篩選
    /// </summary>
    public class ProductSearchCriteria
    {
        public int? ParentCategoryId { get; set; }
        public int? CategoryId { get; set; }
        public string? Keyword { get; set; }
        public int? BrandId { get; set; }
        public int? StoreId { get; set; }
        public int? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        /// <summary>date_desc|date_asc|name_asc|name_desc|price_asc|price_desc|status_asc|status_desc</summary>
        public string? SortOrder { get; set; }
        /// <summary>是否包含已刪除商品（賣家查詢自己的商品時設為 true）</summary>
        public bool IncludeDeleted { get; set; } = false;
    }
}
