using System.ComponentModel.DataAnnotations;

namespace ISpanShop.MVC.Models.ViewModels
{
    public class ProductVariantCreateVm
    {
        [Required]
        public int ProductId { get; set; }

        // 唯讀顯示用（不提交）
        public string ProductName { get; set; } = string.Empty;

        // 商品規格定義 JSON（傳給前端用於渲染下拉選單）
        public string SpecDefinitionJson { get; set; } = "[]";

        // SKU 代碼（選填，空白則自動產生）
        [StringLength(100)]
        public string? SkuCode { get; set; }

        // 由前端根據規格選擇組合後填入
        [Required(ErrorMessage = "請選擇規格")]
        public string VariantName { get; set; } = string.Empty;

        // 規格值 JSON，由前端自動序列化
        [Required]
        public string SpecValueJson { get; set; } = "{}";

        [Required(ErrorMessage = "請輸入售價")]
        [Range(0.01, 9_999_999, ErrorMessage = "售價必須大於 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "請輸入庫存")]
        [Range(0, int.MaxValue, ErrorMessage = "庫存不可為負數")]
        public int Stock { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "安全庫存不可為負數")]
        public int SafetyStock { get; set; } = 5;
    }
}
