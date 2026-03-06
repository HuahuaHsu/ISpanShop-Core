using System.ComponentModel.DataAnnotations;

namespace ISpanShop.MVC.Models.ViewModels
{
    public class ProductVariantEditVm
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        // 顯示用（不可修改）
        public string VariantName { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU 代碼為必填")]
        [StringLength(100, ErrorMessage = "SKU 不可超過 100 字")]
        public string SkuCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入售價")]
        [Range(0.01, 9_999_999, ErrorMessage = "售價必須大於 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "請輸入庫存")]
        [Range(0, int.MaxValue, ErrorMessage = "庫存不可為負數")]
        public int Stock { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "安全庫存不可為負數")]
        public int SafetyStock { get; set; }
    }
}
