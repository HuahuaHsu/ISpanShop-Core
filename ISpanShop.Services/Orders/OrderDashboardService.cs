using ISpanShop.Models.DTOs;
using ISpanShop.Models.DTOs.Orders;
using ISpanShop.Repositories.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Services.Orders
{
	public class OrderDashboardService : IOrderDashboardService
	{
		private readonly IOrderRepository _orderRepository;

		public OrderDashboardService(IOrderRepository orderRepository)
		{
			_orderRepository = orderRepository;
		}

		// 解析 period，回傳當期與前一期的 Date Range
		private (DateTime Start, DateTime End, DateTime PrevStart, DateTime PrevEnd) ParsePeriod(string period)
		{
			var now = DateTime.Now;
			DateTime start = now.Date, end = now, prevStart = now.Date, prevEnd = now;

			switch (period.ToLower())
			{
				case "day": // 今日 vs 昨日
					start = now.Date;
					end = now;
					prevStart = start.AddDays(-1);
					prevEnd = end.AddDays(-1);
					break;
				case "7days":
					start = now.AddDays(-7).Date;
					end = now;
					prevStart = start.AddDays(-7);
					prevEnd = start.AddTicks(-1);
					break;
				case "month": 
					start = now.AddDays(-30).Date;
					end = now;
					prevStart = start.AddDays(-30);
					prevEnd = start.AddTicks(-1);
					break;
				case "3months":
					start = now.AddMonths(-3).Date;
					end = now;
					prevStart = start.AddMonths(-3);
					prevEnd = start.AddTicks(-1);
					break;
				case "6months":
					start = now.AddMonths(-6).Date;
					end = now;
					prevStart = start.AddMonths(-6);
					prevEnd = start.AddTicks(-1);
					break;
				case "year":
					start = now.AddYears(-1).Date;
					end = now;
					prevStart = start.AddYears(-1);
					prevEnd = start.AddTicks(-1);
					break;
				default:
					start = now.AddDays(-30).Date;
					end = now;
					prevStart = start.AddDays(-30);
					prevEnd = start.AddTicks(-1);
					break;
			}

			return (start, end, prevStart, prevEnd);
		}

		// 計算增長率 (%)
		private decimal CalculateGrowthRate(decimal current, decimal previous)
		{
			if (previous == 0) return current > 0 ? 100M : 0M;
			return Math.Round(((current - previous) / previous) * 100, 2);
		}

		public async Task<OrderDashboardKpiDto> GetDashboardKpisAsync(int? storeId, string period)
		{
			var (start, end, prevStart, prevEnd) = ParsePeriod(period);

			// 呼叫 Repository，一次取回本期與上期的原生統計資料
			// DTO 內部包含 CurrentNetRevenue, PrevNetRevenue, TotalOrders 等屬性
			var data = await _orderRepository.GetDashboardKpisAsync(storeId, start, end, prevStart, prevEnd);

			// 計算 KPI 卡片所需最終數據 (C.3, C.5, C.6 等需求)
			return new OrderDashboardKpiDto
			{
				NetRevenue = data.NetRevenue,
				NetRevenueGrowthRate = CalculateGrowthRate(data.NetRevenue, data.PrevNetRevenue),

				PendingShipmentCount = data.PendingShipmentCount,
				PendingRefundCount = data.PendingRefundCount,
				LowStockProductCount = data.LowStockProductCount,

				ItemsPerOrder = data.TotalOrders > 0 ? (decimal)data.TotalItemsSold / data.TotalOrders : 0,
				ItemsPerOrderGrowthRate = CalculateGrowthRate(
					data.TotalOrders > 0 ? (decimal)data.TotalItemsSold / data.TotalOrders : 0,
					data.PrevTotalOrders > 0 ? (decimal)data.PrevTotalItemsSold / data.PrevTotalOrders : 0),

				ReturnRate = data.TotalOrders > 0 ? (decimal)data.ReturnOrders / data.TotalOrders * 100 : 0,
				ReturnRateGrowthRate = CalculateGrowthRate(
					data.TotalOrders > 0 ? (decimal)data.ReturnOrders / data.TotalOrders : 0,
					data.PrevTotalOrders > 0 ? (decimal)data.PrevReturnOrders / data.PrevTotalOrders : 0
				)
			};
		}

		public async Task<ApexChartDataDto> GetProductSalesChartAsync(int? storeId, string period, string chartType)
		{
			var (start, end, _, _) = ParsePeriod(period);

			if (chartType == "Pie")
			{
				return await _orderRepository.GetProductSalesPieChartAsync(storeId, start, end);
			}
			return await _orderRepository.GetProductSalesBarChartAsync(storeId, start, end);
		}

		public async Task<ApexChartDataDto> GetMonthlySalesTrendAsync(int? storeId, int? year)
		{
			// 需求：抓取最近一年的有數據月份 (Rolling 12 Months)
			var now = DateTime.Now;
			var endDate = now;
			var startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-11); // 從 11 個月前的 1 號開始

			return await _orderRepository.GetMonthlySalesTrendAsync(storeId, startDate, endDate);
		}

		public async Task<List<TopProductSalesDto>> GetTop10ProductsAsync(int? storeId, string period, string orderBy)
		{
			var (start, end, _, _) = ParsePeriod(period);
			return await _orderRepository.GetTop10ProductsAsync(storeId, start, end, orderBy);
		}

		public async Task<ApexChartDataDto> GetCategoryContributionAsync(int? storeId, string period)
		{
			var (start, end, _, _) = ParsePeriod(period);
			return await _orderRepository.GetCategoryContributionAsync(storeId, start, end);
		}

		public async Task<ApexChartDataDto> GetCategoryContributionAsync(int? storeId, DateTime start, DateTime end)
		{
			return await _orderRepository.GetCategoryContributionAsync(storeId, start, end);
		}

		public async Task<ApexChartDataDto> GetYearlyRevenueDataAsync(int? storeId, int year)
		{
			var startDate = new DateTime(year, 1, 1);
			var endDate = new DateTime(year, 12, 31, 23, 59, 59);
			return await _orderRepository.GetMonthlySalesTrendAsync(storeId, startDate, endDate);
		}

		public async Task<ApexChartDataDto> GetCategoryDetailAsync(int? storeId, string period, string type, string categoryName)
		{
			var (start, end, _, _) = ParsePeriod(period);
			return await _orderRepository.GetCategoryDetailAsync(storeId, start, end, type, categoryName);
		}

		public async Task<List<CategoryMonthlyDeltaDto>> GetCategoryMonthlyDeltaAsync(int? storeId, int year1, int year2)
		{
			// 抓取兩年內所有訂單的數據 (按月、類別分組)
			var start1 = new DateTime(year1, 1, 1);
			var end1 = new DateTime(year1, 12, 31, 23, 59, 59);
			var start2 = new DateTime(year2, 1, 1);
			var end2 = new DateTime(year2, 12, 31, 23, 59, 59);

			// 直接呼叫 Repository 獲取兩段時間的所有類別/月度數據
			var data1 = await _orderRepository.GetCategoryMonthlyRevenueAsync(storeId, start1, end1);
			var data2 = await _orderRepository.GetCategoryMonthlyRevenueAsync(storeId, start2, end2);

			// 取得所有出現過的類別
			var allCategories = data1.Select(d => d.CategoryName).Union(data2.Select(d => d.CategoryName)).Distinct().ToList();
			var result = new List<CategoryMonthlyDeltaDto>();

			foreach (var cat in allCategories)
			{
				var dto = new CategoryMonthlyDeltaDto { CategoryName = cat };
				for (int m = 1; m <= 12; m++)
				{
					decimal v1 = data1.Where(d => d.CategoryName == cat && d.Month == m).Sum(d => d.Revenue);
					decimal v2 = data2.Where(d => d.CategoryName == cat && d.Month == m).Sum(d => d.Revenue);
					dto.MonthlyDeltas.Add(v1 - v2); // Year1 - Year2
				}
				result.Add(dto);
			}

			return result;
		}
	}
}
