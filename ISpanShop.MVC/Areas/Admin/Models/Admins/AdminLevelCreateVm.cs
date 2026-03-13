using System.ComponentModel.DataAnnotations;

namespace ISpanShop.MVC.Areas.Admin.Models.Admins
{
	public class AdminLevelCreateVm
	{
		[Required(ErrorMessage = "身分名稱為必填")]
		[Display(Name = "身分名稱")]
		public string LevelName { get; set; }

		[Display(Name = "說明")]
		public string Description { get; set; }

		/// <summary>勾選的權限 ID 列表</summary>
		public List<int> PermissionIds { get; set; } = new List<int>();
	}
}