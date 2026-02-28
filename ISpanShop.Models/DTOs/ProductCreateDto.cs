using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ISpanShop.Models.DTOs
{
    /// <summary>
    /// 新增商品用 DTO - 接收完整的商品建立資料
    /// </summary>
    public class ProductCreateDto
    {
        /// <summary>
        /// 店鋪 ID
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// 分類 ID
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 品牌 ID
        /// </summary>
        public int BrandId { get; set; }

        /// <summary>
        /// 商品名稱 - 必填
        /// </summary>
        [Required(ErrorMessage = "商品名稱為必填項")]
        public required string Name { get; set; }

        /// <summary>
        /// 商品描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 影片 URL
        /// </summary>
        public string? VideoUrl { get; set; }

        /// <summary>
        /// 規格定義 JSON
        /// </summary>
        public required string SpecDefinitionJson { get; set; }

        /// <summary>
        /// 規格變體集合 - 用於接收多筆規格
        /// </summary>
        public List<ProductVariantCreateDto> Variants { get; set; } = new List<ProductVariantCreateDto>();

        /// <summary>
        /// 上傳的商品圖片集合 - 用於接收多張圖片
        /// </summary>
        public List<IFormFile> UploadImages { get; set; } = new List<IFormFile>();

        /// <summary>
        /// 主圖索引 - 紀錄在圖片集合中哪一張是主圖
        /// </summary>
        public int MainImageIndex { get; set; }
    }
}
