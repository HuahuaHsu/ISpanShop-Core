using System;

namespace ISpanShop.MVC.Models.ViewModels
{
    /// <summary>
    /// 商品審核列表 ViewModel - 用於待審核商品列表頁面
    /// </summary>
    public class ProductReviewListVm
    {
        /// <summary>
        /// 商品 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 店鋪 ID
        /// </summary>
        public int StoreId { get; set; }

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
        /// 商品狀態 (1=已上架, 2=待審核, 其他值)
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime? CreatedAt { get; set; }
    }
}
