namespace ISpanShop.Models.EfModels;

/// <summary>
/// 擴充自動生成的 CategorySpecMapping，加入在分類中的排序欄位
/// </summary>
public partial class CategorySpecMapping
{
    /// <summary>在該分類中的顯示排序（值越小越前面），對應 DB 欄位 Sort</summary>
    public int Sort { get; set; }
}
