namespace ISpanShop.Models.EfModels;

/// <summary>
/// Product 審核機制擴充欄位（partial class，避免修改 EF Core 自動生成的 Product.cs）
/// ReviewStatus: 0=待審核, 1=審核通過, 2=已退回
/// </summary>
public partial class Product
{
    public int ReviewStatus { get; set; }
    public string? ReviewedBy { get; set; }
    /// <summary>審核時間（通過與退回共用）</summary>
    public DateTime? ReviewDate { get; set; }
}
