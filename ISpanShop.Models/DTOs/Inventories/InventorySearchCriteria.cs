namespace ISpanShop.Models.DTOs.Inventories
{
    public class InventorySearchCriteria
    {
        /// <summary>"" / null = 全部；"low" = 低庫存（含零庫存）；"zero" = 零庫存；"normal" = 正常庫存</summary>
        public string? StockStatus { get; set; }
        public bool LowStockOnly    => StockStatus == "low";
        public bool ZeroStockOnly   => StockStatus == "zero";
        public bool NormalStockOnly => StockStatus == "normal";
        public string? Keyword { get; set; }
        public int? ParentCategoryId { get; set; }
        public int? CategoryId { get; set; }
        public int? StoreId { get; set; }
        public int? MinStock { get; set; }
        public int? MaxStock { get; set; }
        /// <summary>stock_asc | stock_desc | safety_asc | name_asc | default</summary>
        public string? SortBy { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
