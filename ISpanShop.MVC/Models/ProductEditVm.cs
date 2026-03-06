using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ISpanShop.MVC.Models.ViewModels
{
    public class ProductEditVm
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "請選擇商品分類")]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇商品分類")]
        public int CategoryId { get; set; }

        public int? BrandId { get; set; }

        [Required(ErrorMessage = "商品名稱為必填")]
        [StringLength(200, ErrorMessage = "商品名稱不可超過 200 字")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Url(ErrorMessage = "請輸入有效的圖片網址")]
        public string? MainImageUrl { get; set; }

        /// <summary>
        /// 規格維度 JSON（由前端視覺化編輯器序列化後填入）
        /// 格式：[{"name":"顏色","options":["黑","白"]}]
        /// </summary>
        public string? SpecDefinitionJson { get; set; }

        public string? ReturnUrl { get; set; }

        /// <summary>本機上傳圖檔（優先於 MainImageUrl）</summary>
        public IFormFile? ImageFile { get; set; }
    }
}
