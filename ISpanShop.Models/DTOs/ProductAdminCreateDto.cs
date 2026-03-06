namespace ISpanShop.Models.DTOs
{
    /// <summary>
    /// 管理員後台新增商品 DTO（簡化版，略過審核直接上架）
    /// </summary>
    public class ProductAdminCreateDto
    {
        public int StoreId { get; set; }
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? MainImageUrl { get; set; }
    }
}
