using ISpanShop.Common.Enums;
using ISpanShop.Models.DTOs;
using ISpanShop.MVC.Models.Orders;
using ISpanShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISpanShop.MVC.Controllers
{
	public class OrdersController : Controller
	{
		private readonly IOrderService _orderService;
		private readonly IOrderDashboardService _dashboardService;

		public OrdersController(IOrderService orderService, IOrderDashboardService dashboardService)
		{
			_orderService = orderService;
			_dashboardService = dashboardService;
		}

		public async Task<IActionResult> Details(long id)
		{
			var order = await _orderService.GetOrderDetailAsync(id);
			if (order == null) return NotFound();

			var vm = new OrderDetailsVm { Order = order };
			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateStatus(long id, OrderStatus status)
		{
			await _orderService.UpdateStatusAsync(id, status);
			TempData["SuccessMessage"] = $"訂單狀態已更新為 {status.GetDisplayName()}";
			return RedirectToAction(nameof(Details), new { id });
		}
public IActionResult Index()
{
	var vm = new OrderIndexVm
	{
		StatusOptions = Enum.GetValues(typeof(OrderStatus))
			.Cast<OrderStatus>()
			.Select(s => new SelectListItem
			{
				Value = ((byte)s).ToString(),
				Text = s.GetDisplayName()
			}).ToList(),
		DateDimensionOptions = new List<SelectListItem>
		{
			new SelectListItem { Value = "1", Text = "下單日期" },
			new SelectListItem { Value = "2", Text = "付款日期" },
			new SelectListItem { Value = "3", Text = "完成日期" }
		}
	};
	return View(vm);
}

		[HttpPost]
		public async Task<IActionResult> GetOrderListAjax([FromBody] OrderSearchDto searchParams)
		{
			try
			{
				var result = await _orderService.GetFilteredOrdersAsync(searchParams);
				return Json(new { success = true, data = result });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}

		// ============================
		// 頁面渲染：儀表板 (C & D 需求)
		// ============================
		public IActionResult Dashboard()
		{
			var vm = new OrderDashboardVm
			{
				// 初始化選單
				PeriodOptions = new List<SelectListItem>
				{
					new SelectListItem { Value = "7days", Text = "近 7 天" },
					new SelectListItem { Value = "month", Text = "近一個月" },
					new SelectListItem { Value = "3months", Text = "近三個月" }
				}
			};
			return View(vm);
		}

		// AJAX API：取得 KPI 卡片數據
		[HttpGet]
		public async Task<IActionResult> GetDashboardKpis(int? storeId, string period = "month")
		{
			try
			{
				var kpis = await _dashboardService.GetDashboardKpisAsync(storeId, period);
				return Json(kpis);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// AJAX API：取得各商品銷售狀況 (長條圖 / 圓餅圖)
		[HttpGet]
		public async Task<IActionResult> GetProductSalesChart(int? storeId, string period = "month", string type = "Bar")
		{
			var chartData = await _dashboardService.GetProductSalesChartAsync(storeId, period, type);
			return Json(chartData);
		}

		// AJAX API：取得年度月營收趨勢 (折線圖/長條圖通用資料)
		[HttpGet]
		public async Task<IActionResult> GetMonthlyTrend(int? storeId, int? year)
		{
			var data = await _dashboardService.GetMonthlySalesTrendAsync(storeId, year);
			return Json(data);
		}

		// AJAX API：取得熱銷 Top 10
		[HttpGet]
		public async Task<IActionResult> GetTop10Products(int? storeId, string period = "month", string orderBy = "revenue")
		{
			var top10 = await _dashboardService.GetTop10ProductsAsync(storeId, period, orderBy);
			return Json(top10);
		}

		// AJAX API：分類貢獻度
		[HttpGet]
		public async Task<IActionResult> GetCategoryContribution(int? storeId, string period = "month")
		{
			var data = await _dashboardService.GetCategoryContributionAsync(storeId, period);
			return Json(data);
		}
	}
}
