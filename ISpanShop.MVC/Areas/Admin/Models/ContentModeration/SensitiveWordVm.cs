using System.ComponentModel.DataAnnotations;

namespace ISpanShop.MVC.Areas.Admin.Models.ContentModeration
{
	public class SensitiveWordVm
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "請輸入敏感字內容")]
		public string Word { get; set; }

		// --- 新增這一個屬性 ---
		public int? CategoryId { get; set; }

		// 這是原本顯示名稱用的 (例如在 Index 列表顯示「色情」)
		public string? Category { get; set; }

		public bool IsActive { get; set; }

		public DateTime? CreatedTime { get; set; }

		// 如果你的 Index 頁面有顯示數量，可以留著
		public int WordCount { get; set; }
	}
}