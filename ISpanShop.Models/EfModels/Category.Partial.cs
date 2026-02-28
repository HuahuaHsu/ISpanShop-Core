using System.ComponentModel.DataAnnotations.Schema;

namespace ISpanShop.EfModels
{
    /// <summary>
    /// Category 實體的擴充部分 - 不修改資料庫
    /// </summary>
    public partial class Category
    {
        /// <summary>
        /// 預設規格 JSON - 不對應到資料庫
        /// </summary>
        [NotMapped]
        public string DefaultSpecJson { get; set; }
    }
}
