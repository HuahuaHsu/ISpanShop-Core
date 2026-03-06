using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ISpanShop.MVC.Models.ViewModels
{
    public class ProductCreateVm
    {
        [Required(ErrorMessage = "請選擇所屬店家")]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇所屬店家")]
        public int StoreId { get; set; }

        [Required(ErrorMessage = "請選擇商品分類")]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇商品分類")]
        public int CategoryId { get; set; }

        public int? BrandId { get; set; }

        [Required(ErrorMessage = "商品名稱為必填")]
        [StringLength(200, ErrorMessage = "商品名稱不可超過 200 字")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "請輸入基礎售價")]
        [Range(0.01, 9_999_999, ErrorMessage = "售價必須大於 0")]
        public decimal Price { get; set; }

        public string? MainImageUrl { get; set; }

        /// <summary>本機上傳圖檔（優先於 MainImageUrl）</summary>
        public IFormFile? ImageFile { get; set; }
    }
}
