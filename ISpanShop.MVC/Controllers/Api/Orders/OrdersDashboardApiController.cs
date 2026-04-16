using Microsoft.AspNetCore.Mvc;
using ISpanShop.Services.Orders;
using System.Threading.Tasks;
using System;

namespace ISpanShop.MVC.Controllers.Api.Orders
{
	[Route("api/dashboard")]
	[ApiController]
	public class OrdersDashboardApiController : ControllerBase
	{
		private readonly IOrderDashboardService _dashboardService;

		public OrdersDashboardApiController(IOrderDashboardService dashboardService)
		{
			_dashboardService = dashboardService;
		}

		// GET: api/dashboard/kpis?storeId=1&period=month
		[HttpGet("kpis")]
		public async Task<IActionResult> GetDashboardKpis(int? storeId, string period = "month")
		{
			try
			{
				var kpis = await _dashboardService.GetDashboardKpisAsync(storeId, period);
				if (kpis == null) return NotFound(new { message = "無法取得 KPI 數據" });
				return Ok(kpis);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// GET: api/dashboard/category-composition?storeId=1&period=month&type=Bar
		[HttpGet("category-composition")]
		public async Task<IActionResult> GetCategoryCompositionChart(int? storeId, string period = "month", string type = "Bar")
		{
			try
			{
				var chartData = await _dashboardService.GetCategoryCompositionChartAsync(storeId, period, type);
				return Ok(chartData);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// GET: api/dashboard/monthly-trend?storeId=1&year=2024
		[HttpGet("monthly-trend")]
		public async Task<IActionResult> GetMonthlyTrend(int? storeId, int? year)
		{
			try
			{
				var data = await _dashboardService.GetMonthlySalesTrendAsync(storeId, year);
				return Ok(data);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// GET: api/dashboard/top-categories?storeId=1&period=month&orderBy=revenue
		[HttpGet("top-categories")]
		public async Task<IActionResult> GetTopSellingCategories(int? storeId, string period = "month", string orderBy = "revenue")
		{
			try
			{
				var top10 = await _dashboardService.GetTopSellingCategoriesAsync(storeId, period, orderBy);
				return Ok(top10);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// GET: api/dashboard/category-contribution?storeId=1&period=month
		[HttpGet("category-contribution")]
		public async Task<IActionResult> GetCategoryContribution(int? storeId, string period = "month")
		{
			try
			{
				var data = await _dashboardService.GetCategoryContributionAsync(storeId, period);
				return Ok(data);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}
	}
}
