namespace ISpanShop.MVC.Models.Dto
{
    /// <summary>前台品牌列表項目（API 回傳用）</summary>
    public class BrandListItemDto
    {
        public int    Id           { get; set; }
        public string Name         { get; set; } = string.Empty;
        /// <summary>有幾個上架中商品屬於此品牌（若有分類篩選則只算該分類內）</summary>
        public int    ProductCount { get; set; }
    }
}
