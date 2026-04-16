namespace ISpanShop.MVC.Models.Dto
{
    /// <summary>前台商品列表項目（API 回傳用）</summary>
    public class ProductListItemDto
    {
        public int      Id            { get; set; }
        public string   Name          { get; set; } = string.Empty;
        /// <summary>多規格取最低價；無規格時取 Product.MinPrice</summary>
        public decimal  Price         { get; set; }
        /// <summary>原價（目前無此欄位，固定 null）</summary>
        public decimal? OriginalPrice { get; set; }
        public string   ImageUrl      { get; set; } = string.Empty;
        public int      SoldCount     { get; set; }
        public int      TotalStock    { get; set; }
        /// <summary>賣家所在地（Store 目前無 Location 欄位，回傳空字串）</summary>
        public string   Location      { get; set; } = string.Empty;
        public int      CategoryId    { get; set; }
        /// <summary>平均評分（目前無評分聚合，固定 null）</summary>
        public decimal? Rating        { get; set; }
    }

    /// <summary>前台商品列表分頁回傳（data 區塊）</summary>
    public class ProductListResponseDto
    {
        public List<ProductListItemDto> Items    { get; set; } = new();
        public int                      Total    { get; set; }
        public int                      Page     { get; set; }
        public int                      PageSize { get; set; }
    }
}
