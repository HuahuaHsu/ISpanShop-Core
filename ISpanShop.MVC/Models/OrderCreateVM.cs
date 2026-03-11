namespace ISpanShop.MVC.Models
{
	public class OrderCreateVM
	{
		public int UserId { get; set; }
		public int StoreId { get; set; }
		public bool UsePoints { get; set; }
		public decimal TotalAmount { get; set; }
		public string RecipientName { get; set; }
		// 可以加入更多欄位...
	}
}
