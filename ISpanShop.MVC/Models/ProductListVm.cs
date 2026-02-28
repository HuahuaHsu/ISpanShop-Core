using System;

namespace ISpanShop.MVC.Models.ViewModels
{
    /// <summary>
    /// 商品列表 ViewModel - 用於全站商品總覽頁面
    /// </summary>
    public class ProductListVm
    {
        /// <summary>
        /// 商品 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 商店名稱
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// 分類名稱
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 品牌名稱
        /// </summary>
        public string BrandName { get; set; }

        /// <summary>
        /// 商品名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 最低價格
        /// </summary>
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// 最高價格
        /// </summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// 商品狀態 (1=已上架, 2=待審核, 0=下架)
        /// </summary>
        public byte? Status { get; set; }

        /// <summary>
        /// 主圖 URL
        /// </summary>
        public string MainImageUrl { get; set; }
    }
}
