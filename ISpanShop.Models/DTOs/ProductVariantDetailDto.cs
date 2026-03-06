namespace ISpanShop.Models.DTOs
{
    public class ProductVariantDetailDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public required string SkuCode { get; set; }
        public required string VariantName { get; set; }
        public decimal Price { get; set; }
        public int? Stock { get; set; }
        public int? SafetyStock { get; set; }
        public string? SpecValueJson { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
