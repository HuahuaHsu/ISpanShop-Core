using ISpanShop.Models.EfModels.DTOs;
using ISpanShop.MVC.Models.Orders;
using ISpanShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Controllers
{
	public class OrdersController : Controller
	{
		private readonly IOrderService _orderService;

		public OrdersController(IOrderService orderService)
		{
			_orderService = orderService;
		}

		public async Task<IActionResult> Index(OrderSearchDto search)
		{
			search.PageNumber = search.PageNumber <= 0 ? 1 : search.PageNumber;
			search.PageSize = search.PageSize <= 0 ? 10 : search.PageSize;

			var result = await _orderService.GetOrdersAsync(search);

			var vm = new OrderIndexVm
			{
				Orders = result,
				Criteria = search
			};

			return View(vm);
		}
	}
}
