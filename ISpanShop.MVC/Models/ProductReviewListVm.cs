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
        /// 商店名稱
        /// </summary>
        public string? StoreName { get; set; }

        /// <summary>
        /// 商品名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 商品描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 商品狀態 (0=下架, 1=上架, 2=待審核, 3=審核退回)
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// 退回原因（Status==3 時填入）
        /// </summary>
        public string? RejectReason { get; set; }

        /// <summary>
        /// 最後更新時間（退回時間）
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 主圖 URL
        /// </summary>
        public string? MainImageUrl { get; set; }
    }
}
