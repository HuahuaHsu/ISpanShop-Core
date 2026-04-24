using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ISpanShop.Models.DTOs.Products
{
    // ════════════════════════════════════════
    // Request DTOs
    // ════════════════════════════════════════

    /// <summary>新增商品（multipart/form-data）</summary>
    public class CreateProductRequest
    {
        [Required]
        public int StoreId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public int? BrandId { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? VideoUrl { get; set; }

        /// <summary>規格維度定義 JSON，例如 [{"name":"顏色"},{"name":"尺寸"}]</summary>
        public string? SpecDefinitionJson { get; set; }

        /// <summary>商品圖片（可多張）</summary>
        public List<IFormFile>? Images { get; set; }

        /// <summary>主圖索引（0-based）</summary>
        public int MainImageIndex { get; set; } = 0;

        /// <summary>儲存模式：draft=草稿, submit=送審</summary>
        public string Mode { get; set; } = "draft";

        /// <summary>變體列表 JSON</summary>
        public string? VariantsJson { get; set; }

        /// <summary>屬性列表 JSON</summary>
        public string? AttributesJson { get; set; }
    }

    /// <summary>更新商品基本資料（JSON）</summary>
    public class UpdateProductRequest
    {
        [Required]
        public int CategoryId { get; set; }

        public int? BrandId { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? SpecDefinitionJson { get; set; }

        public string? MainImageUrl { get; set; }

        /// <summary>儲存模式：draft=草稿, submit=送審</summary>
        public string Mode { get; set; } = "draft";

        /// <summary>變體列表 JSON</summary>
        public string? VariantsJson { get; set; }

        /// <summary>屬性列表 JSON</summary>
        public string? AttributesJson { get; set; }
    }

    /// <summary>屬性資料 DTO</summary>
    public class ProductAttributeDto
    {
        public int AttributeId { get; set; }
        public int? OptionId { get; set; }
        public string? CustomValue { get; set; }
    }

    /// <summary>更新商品狀態（上架/下架）</summary>
    public class UpdateProductStatusRequest
    {
        [Required]
        [Range(0, 1, ErrorMessage = "狀態只能是 0 (下架) 或 1 (上架)")]
        public byte Status { get; set; }
    }

    /// <summary>新增規格</summary>
    public class CreateVariantRequest
    {
        public string? SkuCode { get; set; }

        [Required]
        public string VariantName { get; set; } = string.Empty;

        /// <summary>規格值 JSON，例如 {"顏色":"紅","尺寸":"M"}</summary>
        [Required]
        public string SpecValueJson { get; set; } = string.Empty;

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "售價必須大於 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Range(0, int.MaxValue)]
        public int SafetyStock { get; set; }
    }

    /// <summary>更新規格</summary>
    public class UpdateVariantRequest
    {
        public string? SkuCode { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "售價必須大於 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Range(0, int.MaxValue)]
        public int SafetyStock { get; set; }
    }

    // ════════════════════════════════════════
    // Response DTOs
    // ════════════════════════════════════════

    /// <summary>商品列表項目</summary>
    public class ProductListItemResponse
    {
        public int      Id           { get; set; }
        public string   Name         { get; set; } = string.Empty;
        public string?  StoreName    { get; set; }
        public string?  CategoryName { get; set; }
        public string?  BrandName    { get; set; }
        public decimal? MinPrice     { get; set; }
        public decimal? MaxPrice     { get; set; }
        public byte?    Status       { get; set; }
        public string   StatusText   { get; set; } = string.Empty;
        public string?  MainImageUrl { get; set; }
        public DateTime? CreatedAt   { get; set; }
        public int      TotalStock    { get; set; }
        public int?     TotalSales    { get; set; }
        public int?     ViewCount     { get; set; }
        /// <summary>審核退回原因（status=3 時才有值）</summary>
        public string?  RejectReason  { get; set; }
        /// <summary>審核狀態：0=待審核 1=通過 2=退回 3=重新送審</summary>
        public int      ReviewStatus  { get; set; }
        public bool     IsDeleted     { get; set; }
    }

    /// <summary>商品詳情（含規格列表）</summary>
    public class ProductDetailResponse
    {
        public int      Id                 { get; set; }
        public string   Name               { get; set; } = string.Empty;
        public int      StoreId            { get; set; }
        public string?  StoreName          { get; set; }
        public int      CategoryId         { get; set; }
        public string?  CategoryName       { get; set; }
        public int?     BrandId            { get; set; }
        public string?  BrandName          { get; set; }
        public string?  Description        { get; set; }
        public byte?    Status             { get; set; }
        public string   StatusText         { get; set; } = string.Empty;
        public decimal? MinPrice           { get; set; }
        public decimal? MaxPrice           { get; set; }
        public string?  SpecDefinitionJson { get; set; }
        public string?  RejectReason       { get; set; }
        /// <summary>審核狀態：0=待審核 1=通過 2=退回 3=重新送審</summary>
        public int      ReviewStatus       { get; set; }
        public DateTime? CreatedAt         { get; set; }
        public DateTime? UpdatedAt         { get; set; }
        public string?   AttributesJson     { get; set; }
        public List<string>                  Images   { get; set; } = new();
        public List<VariantDetailResponse>   Variants { get; set; } = new();
    }

    /// <summary>規格詳情</summary>
    public class VariantDetailResponse
    {
        public int      Id            { get; set; }
        public string?  SkuCode       { get; set; }
        public string?  VariantName   { get; set; }
        public decimal  Price         { get; set; }
        public int?     Stock         { get; set; }
        public int?     SafetyStock   { get; set; }
        public string?  SpecValueJson { get; set; }
    }
}
