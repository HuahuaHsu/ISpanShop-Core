namespace ISpanShop.Models.DTOs.Brands
{
    /// <summary>品牌 + 上架商品數（Repository → Service 傳遞用）</summary>
    public class BrandWithCountDto
    {
        public int    Id           { get; set; }
        public string Name         { get; set; } = string.Empty;
        public int    ProductCount { get; set; }
    }
}
