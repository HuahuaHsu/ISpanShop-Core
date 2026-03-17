using System;
using System.Collections.Generic;

namespace ISpanShop.Models.DTOs.Products
{
    public class ProductDetailDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int StoreId { get; set; }
        public string? StoreName { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public string? Description { get; set; }
        public byte? Status { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? TotalSales { get; set; }
        public int? ViewCount { get; set; }
        public string? RejectReason { get; set; }
        /// <summary>審核狀態 (0=待審核, 1=通過, 2=退回, 3=重新申請審核)</summary>
        public int ReviewStatus { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string? ForceOffShelfReason { get; set; }
        public DateTime? ForceOffShelfDate { get; set; }
        public int? ForceOffShelfBy { get; set; }
        public DateTime? ReApplyDate { get; set; }
        public string? SpecDefinitionJson { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> Images { get; set; } = new();
        public List<ProductVariantDetailDto> Variants { get; set; } = new();
    }
}
