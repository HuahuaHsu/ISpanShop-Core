namespace ISpanShop.Models.DTOs
{
    public class InventoryListDto
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public string SkuCode { get; set; } = string.Empty;
        public int Stock { get; set; }
        public int SafetyStock { get; set; }
        public bool IsZeroStock => Stock == 0;
        public bool IsLowStock  => Stock > 0 && Stock <= SafetyStock;
    }
}
