using System.ComponentModel.DataAnnotations;

namespace ISpanShop.Models.DTOs
{
	/// <summary>
	/// 管理員資料傳輸物件 - 用於列表、詳細資訊及編輯
	/// </summary>
	public class AdminDto
	{
		[Display(Name = "管理員ID")]
		public int AdminId { get; set; }

		[Display(Name = "帳號")]
		public string Account { get; set; }

		[Display(Name = "電子信箱")]
		public string Email { get; set; }

		[Display(Name = "角色ID")]
		public int RoleId { get; set; }

		[Display(Name = "角色名稱")]
		public string RoleName { get; set; }

		[Display(Name = "建立時間")]
		public DateTime? CreatedAt { get; set; }

		[Display(Name = "更新時間")]
		public DateTime? UpdatedAt { get; set; }
	}

	/// <summary>
	/// 角色資料傳輸物件 - 用於下拉選單
	/// </summary>
	public class AdminPermissionDto
	{
		[Display(Name = "角色ID")]
		public int PermissionId { get; set; }

		[Display(Name = "角色名稱")]
		public string PermissionKey { get; set; }

		[Display(Name = "描述")]
		public string Description { get; set; }
	}
}
