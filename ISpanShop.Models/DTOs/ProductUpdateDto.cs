namespace ISpanShop.Models.DTOs
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
    }
}
