using System;

namespace ISpanShop.Models.DTOs.Products
{
    /// <summary>
    /// 商品列表 DTO - Service 層回傳的資料轉移物件
    /// </summary>
    public class ProductListDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int? TotalSales { get; set; }
        public int TotalStock { get; set; }
        public string? StoreName { get; set; }
        public required string CategoryName { get; set; }
        public string? BrandName { get; set; }
        public required string Name { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        /// <summary>商品狀態 (1=已上架, 2=待審核, 0=下架)</summary>
        public byte? Status { get; set; }
        public required string MainImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        /// <summary>審核狀態 (0=待審核, 1=通過, 2=退回, 3=重新申請審核)</summary>
        public int ReviewStatus { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string? RejectReason { get; set; }
        /// <summary>強制下架原因（Status==4 時）</summary>
        public string? ForceOffShelfReason { get; set; }
        public DateTime? ReApplyDate { get; set; }
    }
}
