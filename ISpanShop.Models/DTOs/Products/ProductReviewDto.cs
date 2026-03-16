using System;

namespace ISpanShop.Models.DTOs.Products
{
    /// <summary>
    /// 商品審核 DTO - Service 層回傳的資料轉移物件
    /// </summary>
    public class ProductReviewDto
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public required string CategoryName { get; set; }
        public string? BrandName { get; set; }
        public string? StoreName { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        /// <summary>商品狀態 (0=下架, 1=上架, 2=待審核, 3=審核退回)</summary>
        public byte Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? RejectReason { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? MainImageUrl { get; set; }
        /// <summary>審核狀態 (0=待審核, 1=通過, 2=退回, 3=重新申請審核)</summary>
        public int ReviewStatus { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewDate { get; set; }
        /// <summary>強制下架原因（Status==4 時）</summary>
        public string? ForceOffShelfReason { get; set; }
        /// <summary>強制下架時間</summary>
        public DateTime? ForceOffShelfDate { get; set; }
        /// <summary>執行強制下架的管理員</summary>
        public int? ForceOffShelfBy { get; set; }
        /// <summary>賣家申請重新上架的時間</summary>
        public DateTime? ReApplyDate { get; set; }
    }
}
