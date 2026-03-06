using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISpanShop.MVC.Models.Orders
{
	public class OrderDashboardVm
	{
		public List<SelectListItem> StoreOptions { get; set; } = new List<SelectListItem>();
		public List<SelectListItem> PeriodOptions { get; set; } = new List<SelectListItem>();
	}
}
