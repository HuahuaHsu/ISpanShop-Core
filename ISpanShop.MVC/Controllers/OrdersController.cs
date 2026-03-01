using ISpanShop.Common.Enums;
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


		// GET: Orders/Details/5
		public async Task<IActionResult> Details(long id)
		{
			var order = await _orderService.GetOrderDetailAsync(id);
			if (order == null) return NotFound();

			var vm = new OrderDetailsVm { Order = order };
			return View(vm);
		}


		// POST: Orders/UpdateStatus
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateStatus(long id, OrderStatus status)
		{
			await _orderService.UpdateStatusAsync(id, status);

			// 更新後導回明細頁，並可顯示提示訊息
			TempData["SuccessMessage"] = $"訂單狀態已更新為 {status}";
			return RedirectToAction(nameof(Details), new { id });
		}
	}
}
