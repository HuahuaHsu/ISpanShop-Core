namespace ISpanShop.MVC.Models.Dto
{
    /// <summary>前台進行中活動列表項目（API 回傳用）</summary>
    public class PromotionListItemDto
    {
        public int      Id             { get; set; }
        /// <summary>活動名稱（對應 DB Promotion.Name）</summary>
        public string   Title          { get; set; } = string.Empty;
        /// <summary>活動描述（對應 DB Promotion.Description），無則 null</summary>
        public string?  Subtitle       { get; set; }
        /// <summary>活動類型英文代號：flashSale / discount / limitedBuy</summary>
        public string   Type           { get; set; } = string.Empty;
        /// <summary>活動類型中文標籤</summary>
        public string   TypeLabel      { get; set; } = string.Empty;
        /// <summary>橫幅圖片：取第一筆關聯商品的主圖</summary>
        public string?  BannerImageUrl { get; set; }
        /// <summary>關聯商品主圖列表（最多 4 張，用於前端輪播背景）</summary>
        public List<string> ProductImages { get; set; } = new();
        /// <summary>點擊跳轉路徑（前端慣例 /promotion/{id}）</summary>
        public string   LinkUrl        { get; set; } = string.Empty;
        public DateTime StartDate      { get; set; }
        public DateTime EndDate        { get; set; }
    }
}
