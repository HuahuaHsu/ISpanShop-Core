using ISpanShop.Models.DTOs;
using System.Collections.Generic;

namespace ISpanShop.MVC.Models.Admins
{
	/// <summary>
	/// 管理員列表 ViewModel
	/// </summary>
	public class AdminIndexVm
	{
		/// <summary>
		/// 管理員列表
		/// </summary>
		public List<AdminDto> Admins { get; set; } = new List<AdminDto>();

		/// <summary>
		/// 角色下拉選單來源
		/// </summary>
		public List<AdminPermissionDto> PermissionOptions { get; set; } = new();

		/// <summary>
		/// 操作結果訊息（成功/失敗）
		/// </summary>
		public string Message { get; set; }
	}
}
