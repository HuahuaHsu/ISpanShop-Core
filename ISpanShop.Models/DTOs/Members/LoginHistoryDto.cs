using System.ComponentModel.DataAnnotations;

namespace ISpanShop.Models.DTOs.Members
{
	/// <summary>
	/// 登入紀錄資料傳輸物件 - 用於後台管理列表
	/// </summary>
	public class LoginHistoryDto
	{
		[Display(Name = "紀錄ID")]
		public int Id { get; set; }

		[Display(Name = "使用者ID")]
		public int? UserId { get; set; }

		[Display(Name = "登入嘗試帳號")]
		public string AttemptedAccount { get; set; }

		[Display(Name = "使用者帳號")]
		public string UserAccount { get; set; }

		[Display(Name = "登入時間")]
		public DateTime LoginTime { get; set; }

		[Display(Name = "IP位址")]
		public string Ipaddress { get; set; }

		[Display(Name = "是否成功")]
		public bool IsSuccess { get; set; }
	}
}
