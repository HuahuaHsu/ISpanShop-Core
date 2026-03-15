namespace ISpanShop.Models.DTOs.Admins
{
	public class AdminCriteria
	{
		public string Keyword { get; set; }
		public string Status { get; set; } // "all", "active", "blocked", "firstLogin"
		public int? AdminLevelId { get; set; }
		public string SortColumn { get; set; } = "UserId";
		public bool IsAscending { get; set; } = true;

		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}
}
