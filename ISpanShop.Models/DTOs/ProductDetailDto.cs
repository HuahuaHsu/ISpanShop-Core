using System;
using System.Collections.Generic;

namespace ISpanShop.Models.DTOs
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
        public string? SpecDefinitionJson { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<string> Images { get; set; } = new();
        public List<ProductVariantDetailDto> Variants { get; set; } = new();
    }
}
