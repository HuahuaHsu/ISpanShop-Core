using ISpanShop.Common.Enums;
using ISpanShop.Models.DTOs.Orders;
using ISpanShop.MVC.Areas.Admin.Controllers;
using ISpanShop.MVC.Areas.Admin.Models.Orders;
using ISpanShop.Services.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Orders
{
	public class OrdersController : AdminBaseController
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
		public async Task<IActionResult> Index()
		{
			var counts = await _orderService.GetOrderStatusCountsAsync();

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
				},
				CountTotal = counts.Values.Sum(),
				CountPendingPayment = counts.TryGetValue(0, out int cp) ? cp : 0,
				CountPendingShipment = counts.TryGetValue(1, out int cs) ? cs : 0,
				CountCompleted = counts.TryGetValue(3, out int cc) ? cc : 0
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
					new SelectListItem { Value = "3months", Text = "近三個月" },
					new SelectListItem { Value = "6months", Text = "近六個月" },
					new SelectListItem { Value = "year", Text = "近一年" }
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

		[HttpGet]
		public async Task<IActionResult> GetYearOverYearComparison(int year1, int year2, int? storeId)
		{
			var data1 = await _dashboardService.GetYearlyRevenueDataAsync(storeId, year1);
			var data2 = await _dashboardService.GetYearlyRevenueDataAsync(storeId, year2);

			var series1 = new decimal[12];
			var series2 = new decimal[12];
			decimal total1 = 0;
			decimal total2 = 0;

			for (int i = 0; i < data1.Labels.Count; i++)
			{
				var month = int.Parse(data1.Labels[i].Split('/')[1]);
				var val = data1.Series[0].Data[i];
				if (month >= 1 && month <= 12) series1[month - 1] = val;
				total1 += val;
			}

			for (int i = 0; i < data2.Labels.Count; i++)
			{
				var month = int.Parse(data2.Labels[i].Split('/')[1]);
				var val = data2.Series[0].Data[i];
				if (month >= 1 && month <= 12) series2[month - 1] = -val;
				total2 += val;
			}

			decimal growthRate = total2 == 0 ? (total1 > 0 ? 100 : 0) : Math.Round(((total1 - total2) / total2) * 100, 1);

			return Json(new
			{
				labels = new[] { "1月", "2月", "3月", "4月", "5月", "6月", "7月", "8月", "9月", "10月", "11月", "12月" },
				series = new[]
				{
					new { name = year1.ToString(), data = series1 },
					new { name = year2.ToString(), data = series2 }
				},
				total1,
				total2,
				growthRate
			});
		}
	}
}
