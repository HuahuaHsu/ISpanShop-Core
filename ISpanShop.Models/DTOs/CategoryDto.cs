namespace ISpanShop.Models.DTOs
{
    /// <summary>
    /// 分類 DTO - 用於前端下拉選單
    /// </summary>
    public class CategoryDto
    {
        /// <summary>
        /// 分類 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 分類名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 父分類 ID（null 表示為主分類）
        /// </summary>
        public int? ParentId { get; set; }
    }
}
