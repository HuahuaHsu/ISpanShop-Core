using System.ComponentModel.DataAnnotations;

namespace ISpanShop.Models.DTOs
{
	/// <summary>
	/// 登入紀錄查詢條件 - 用於搜尋、排序、分頁
	/// </summary>
	public class LoginHistoryCriteria
	{
		/// <summary>
		/// 文字搜尋：針對帳號或 IP
		/// </summary>
		[Display(Name = "搜尋")]
		public string Keyword { get; set; }

		/// <summary>
		/// 篩選條件：登入成功與否的狀態
		/// null = 全部, true = 成功, false = 失敗
		/// </summary>
		[Display(Name = "登入狀態")]
		public bool? IsSuccessful { get; set; }

		/// <summary>
		/// 排序欄位名稱 (例如: LoginTime, UserAccount, Ipaddress)
		/// </summary>
		public string SortColumn { get; set; } = "LoginTime";

		/// <summary>
		/// 是否為升冪排序 (true = 升序, false = 降序)
		/// </summary>
		public bool IsAscending { get; set; } = false;

		/// <summary>
		/// 目前頁碼 (從 1 開始)
		/// </summary>
		[Display(Name = "頁碼")]
		public int PageNumber { get; set; } = 1;

		/// <summary>
		/// 每頁筆數
		/// </summary>
		[Display(Name = "每頁筆數")]
		public int PageSize { get; set; } = 10;

		/// <summary>
		/// 驗證並修正參數
		/// </summary>
		public void Validate()
		{
			if (PageNumber < 1) PageNumber = 1;
			if (PageSize < 1) PageSize = 10;
			if (PageSize > 100) PageSize = 100;  // 最多一次取 100 筆
		}
	}
}
