namespace ISpanShop.Models.DTOs.Members
{
	public class MemberCriteria
	{
		public string Keyword { get; set; }
		public string Status { get; set; }
		public int? RoleId { get; set; } // 1: Buyer, 2: Seller (based on IsSeller logic)
		public int? LevelId { get; set; }
		public string SortColumn { get; set; } = "Id";
		public bool IsAscending { get; set; } = true;
		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}
}
