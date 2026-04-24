using System;
using System.Collections.Generic;

namespace ISpanShop.MVC.Models.Dto
{
    /// <summary>前台商品詳情頁完整資料（API 回傳用）</summary>
    public class ProductDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public List<CategoryPathItemDto> CategoryPath { get; set; } = new();
        public BrandInfoDto? Brand { get; set; }
        public StoreInfoDto Store { get; set; } = null!;
        public List<ProductImageDto> Images { get; set; } = new();
        public PriceRangeDto PriceRange { get; set; } = null!;
        /// <summary>原價區間（目前 DB 無此欄位，回 null）</summary>
        public PriceRangeDto? OriginalPriceRange { get; set; }
        public decimal? DiscountRate { get; set; }
        public List<ProductSpecDto> Specs { get; set; } = new();
        public List<ProductVariantDto> Variants { get; set; } = new();
        public int TotalStock { get; set; }
        public int SoldCount { get; set; }
        /// <summary>商品評分（由 OrderReview 聚合）</summary>
        public decimal? Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsOnShelf { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int ViewCount { get; set; }
        /// <summary>分類動態屬性 JSON（由賣家設定，格式：[{"AttributeId":1,"OptionId":2,"CustomValue":null}]）</summary>
        public string? AttributesJson { get; set; }
    }

    public class CategoryPathItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class BrandInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
    }

    public class StoreInfoDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Status { get; set; } // 1: 營業中, 2: 休假中
        /// <summary>Store 目前無 LogoUrl 欄位，回 null</summary>
        public string? LogoUrl { get; set; }
        /// <summary>Store 目前無 Rating 欄位，回 null</summary>
        public decimal? Rating { get; set; }
        /// <summary>上架中商品數，從 DB 計算</summary>
        public int? ProductCount { get; set; }
        /// <summary>Store 目前無 FollowerCount 欄位，回 null</summary>
        public int? FollowerCount { get; set; }
        /// <summary>Store 目前無 Location 欄位，回 null</summary>
        public string? Location { get; set; }
        /// <summary>Store 目前無 ResponseRate 欄位，回 null</summary>
        public int? ResponseRate { get; set; }
        /// <summary>由 Store.CreatedAt 計算</summary>
        public int? JoinedYearsAgo { get; set; }
    }

    public class ProductImageDto
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public int SortOrder { get; set; }
    }

    public class PriceRangeDto
    {
        public decimal Min { get; set; }
        public decimal Max { get; set; }
    }

    public class ProductSpecDto
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
    }

    public class ProductVariantDto
    {
        public int Id { get; set; }
        public Dictionary<string, string> SpecValues { get; set; } = new();
        public decimal Price { get; set; }
        /// <summary>原價（目前 DB 無此欄位，回 null）</summary>
        public decimal? OriginalPrice { get; set; }
        public int Stock { get; set; }
        /// <summary>該規格對應圖片（ProductImage.VariantId 對應）</summary>
        public string? ImageUrl { get; set; }
    }
}
