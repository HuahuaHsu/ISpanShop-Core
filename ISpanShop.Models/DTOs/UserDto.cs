using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.DTOs
{
	public class UserDto
	{
		public int Id { get; set; }

		[Display(Name = "帳號")]
		public string Account { get; set; }

		[Display(Name = "電子信箱")]
		public string Email { get; set; }

		// 這裡處理 bool? 轉 bool 的邏輯，或者直接給 View 顯示用的字串
		[Display(Name = "狀態")]
		public bool? IsBlacklisted { get; set; }

		[Display(Name = "狀態")]
		public string IsBlacklistedText => IsBlacklisted == true ? "黑名單" : "正常";

		[Display(Name = "賣家身分")]
		public bool IsSeller { get; set; }

		// 直接把 RoleName 拉平放在這裡，View 就不用再 .Role.RoleName
		[Display(Name = "權限角色")]
		public string RoleName { get; set; }
	}
}
