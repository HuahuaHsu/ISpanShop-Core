namespace ISpanShop.Models.EfModels;

/// <summary>
/// Product 審核機制擴充（屬性已在自動生成的 Product.cs 中定義）
/// </summary>
public partial class Product
{
    // 這裡不需要重複定義 ReviewStatus, ReviewedBy, ReviewDate
    // 因為 EF Core Power Tools 已經在 Product.cs 幫您產生了
    
    // 您可以在這裡加入額外的唯讀屬性或方法，例如：
    // public string ReviewStatusText => ReviewStatus == 1 ? "通過" : "待審核";
}
