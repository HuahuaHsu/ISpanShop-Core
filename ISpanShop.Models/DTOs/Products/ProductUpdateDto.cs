namespace ISpanShop.Models.DTOs.Products
{
    /// <summary>
    /// 管理員後台編輯商品 DTO
    /// </summary>
    public class ProductUpdateDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? SpecDefinitionJson { get; set; }
        public string? MainImageUrl { get; set; }
        public byte? Status { get; set; }
        public int? ReviewStatus { get; set; }
        public string? VariantsJson { get; set; }
        public string? AttributesJson { get; set; }
    }
}
