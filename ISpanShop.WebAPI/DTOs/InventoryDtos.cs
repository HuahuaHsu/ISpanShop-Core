using System.ComponentModel.DataAnnotations;

namespace ISpanShop.WebAPI.DTOs
{
    // ════════════════════════════════════════
    // Request DTOs
    // ════════════════════════════════════════

    /// <summary>調整現有庫存</summary>
    public class UpdateStockRequest
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "庫存數量不可為負數")]
        public int Stock { get; set; }
    }

    /// <summary>調整安全庫存</summary>
    public class UpdateSafetyStockRequest
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "安全庫存不可為負數")]
        public int SafetyStock { get; set; }
    }

    /// <summary>同時調整現有庫存與安全庫存</summary>
    public class UpdateInventoryRequest
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "庫存數量不可為負數")]
        public int Stock { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "安全庫存不可為負數")]
        public int SafetyStock { get; set; }
    }

    // ════════════════════════════════════════
    // Response DTOs
    // ════════════════════════════════════════

    /// <summary>庫存列表項目</summary>
    public class InventoryItemDto
    {
        public int    VariantId    { get; set; }
        public int    ProductId    { get; set; }
        public string ProductName  { get; set; } = string.Empty;
        public string VariantName  { get; set; } = string.Empty;
        public string SkuCode      { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int    Stock        { get; set; }
        public int    SafetyStock  { get; set; }
        /// <summary>normal | low | outOfStock</summary>
        public string Status       { get; set; } = "normal";
    }

    /// <summary>單一規格詳情（包含賣家資訊）</summary>
    public class InventoryDetailDto : InventoryItemDto
    {
        public string StoreName { get; set; } = string.Empty;
    }

    /// <summary>庫存統計摘要</summary>
    public class InventorySummaryDto
    {
        public int TotalVariants  { get; set; }
        public int LowStockCount  { get; set; }
        public int ZeroStockCount { get; set; }
        public int NormalCount    { get; set; }
    }

    /// <summary>通用分頁結果（前台 API 版，欄位語意更清晰）</summary>
    public class PagedResultDto<T>
    {
        public List<T> Items      { get; set; } = new();
        public int     TotalCount { get; set; }
        public int     Page       { get; set; }
        public int     PageSize   { get; set; }
        public int     TotalPages { get; set; }
    }
}
