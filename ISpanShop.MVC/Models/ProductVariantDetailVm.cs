using System.Collections.Generic;

namespace ISpanShop.MVC.Models.ViewModels
{
    public class ProductVariantDetailVm
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? SkuCode { get; set; }
        public string? VariantName { get; set; }
        public decimal Price { get; set; }
        public int? Stock { get; set; }
        public int? SafetyStock { get; set; }
        public string? SpecValueJson { get; set; }
        public bool IsDeleted { get; set; }

        public Dictionary<string, string> SpecValues
        {
            get
            {
                if (string.IsNullOrEmpty(SpecValueJson)) return new();
                try
                {
                    return System.Text.Json.JsonSerializer
                        .Deserialize<Dictionary<string, string>>(SpecValueJson)
                        ?? new();
                }
                catch { return new(); }
            }
        }
    }
}
