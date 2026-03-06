namespace ISpanShop.Models.DTOs
{
    public class ProductVariantUpdateDto
    {
        public int Id { get; set; }
        public string SkuCode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int SafetyStock { get; set; }
    }
}
