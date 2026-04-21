using System.ComponentModel.DataAnnotations;

namespace ISpanShop.Models.DTOs.Members
{
	public class MemberDto
	{
		public int Id { get; set; }

		// --- 來自 Users 表 ---
		[Display(Name = "帳號")]
		public string Account { get; set; }

		[Display(Name = "電子信箱")]
		public string Email { get; set; }

		[Display(Name = "黑名單")]
		public bool IsBlacklisted { get; set; }

		[Display(Name = "賣家身分")]
		public bool IsSeller { get; set; }

		// --- 來自 MemberProfiles 表 ---
		[Display(Name = "姓名")]
		public string FullName { get; set; }

		[Display(Name = "電話")]
		public string PhoneNumber { get; set; }

		[Display(Name = "性別")]
		public byte? Gender { get; set; }

		[Display(Name = "生日")]
		public System.DateOnly? Birthday { get; set; }

		[Display(Name = "點數")]
		public int PointBalance { get; set; }

		[Display(Name = "累計消費")]
		public decimal TotalSpending { get; set; }

		[Display(Name = "頭像")]
		public string AvatarUrl { get; set; } // 儲存於 MemberProfiles 表的頭像路徑

		// --- 來自 MembershipLevels 表 ---
		[Display(Name = "會員等級")]
		public string LevelName { get; set; }

		// --- 來自 Addresses 表 ---
		public string City { get; set; }
		public string Region { get; set; }
		public string Address { get; set; } // 對應 Street 欄位

		public string RoleName { get; set; }
	}
}