using ISpanShop.Models.DTOs.Admins;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ISpanShop.MVC.Areas.Admin.Models.Admins
{
	/// <summary>
	/// 新增管理員 ViewModel
	/// </summary>
	public class AdminCreateVm
	{
		/// <summary>
		/// 初始密碼（系統自動生成或管理員輸入）
		/// </summary>
		[Required(ErrorMessage = "密碼為必填")]
		[StringLength(50, MinimumLength = 8, ErrorMessage = "密碼長度應為 8-50 個字元")]
		[Display(Name = "初始密碼")]
		public string Password { get; set; }

		/// <summary>
		/// 管理員等級 ID（不含超級管理員）
		/// </summary>
		[Required(ErrorMessage = "管理員等級為必填")]
		[Display(Name = "管理員等級")]
		public int AdminLevelId { get; set; }

		/// <summary>
		/// 管理員等級下拉選單來源
		/// </summary>
		public List<AdminLevelDto> AdminLevelOptions { get; set; } = new List<AdminLevelDto>();

		// 帳號 / Email / RoleId 皆由系統自動處理
	}
}
