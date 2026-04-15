namespace ISpanShop.MVC.Models.Dto
{
    /// <summary>前台主分類列表項目（API 回傳用）</summary>
    public class CategoryListItemDto
    {
        public int     Id           { get; set; }
        public string  Name         { get; set; } = string.Empty;
        /// <summary>父分類 Id；主分類時為 null，子分類時為父 Id</summary>
        public int?    ParentId     { get; set; }
        /// <summary>分類圖示 URL（對應 DB 欄位 IconUrl），無則 null</summary>
        public string? IconUrl      { get; set; }
        /// <summary>排序權重（對應 DB 欄位 Sort）</summary>
        public int     SortOrder    { get; set; }
        /// <summary>該分類底下有幾個上架中商品</summary>
        public int     ProductCount { get; set; }
    }
}
