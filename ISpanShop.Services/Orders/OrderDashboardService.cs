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
			var data = await _orderRepository.GetDashboardKpisAsync(storeId, start, end, prevStart, prevEnd);

			// A. 營收與訂單指標
			decimal revenue = data.NetRevenue;
			decimal prevRevenue = data.PrevNetRevenue;
			decimal itemsPerOrder = data.TotalOrders > 0 ? (decimal)data.TotalItemsSold / data.TotalOrders : 0;
			decimal prevItemsPerOrder = data.PrevTotalOrders > 0 ? (decimal)data.PrevTotalItemsSold / data.PrevTotalOrders : 0;
			decimal returnRate = data.TotalOrders > 0 ? (decimal)data.ReturnOrders / data.TotalOrders * 100 : 0;
			decimal prevReturnRate = data.PrevTotalOrders > 0 ? (decimal)data.PrevReturnOrders / data.PrevTotalOrders * 100 : 0;

			// 客單價 (AOV)
			decimal aov = data.TotalOrders > 0 ? data.NetRevenue / data.TotalOrders : 0;
			decimal prevAov = data.PrevTotalOrders > 0 ? data.PrevNetRevenue / data.PrevTotalOrders : 0;

			// B. 顧客行為
			decimal repeatRate = data.UniqueMemberCount > 0 ? (decimal)data.RepeatMemberCount / data.UniqueMemberCount * 100 : 0;
			decimal prevRepeatRate = data.PrevUniqueMemberCount > 0 ? (decimal)data.PrevRepeatMemberCount / data.PrevUniqueMemberCount * 100 : 0;

			// C. 營運效率 (出貨天數)
			double curAvgTicks = data.ShippedOrderCount > 0 ? data.TotalFulfillmentTicks / data.ShippedOrderCount : 0;
			double prevAvgTicks = data.PrevShippedOrderCount > 0 ? data.PrevTotalFulfillmentTicks / data.PrevShippedOrderCount : 0;
			decimal curAvgDays = Math.Round((decimal)TimeSpan.FromTicks((long)curAvgTicks).TotalDays, 1);
			decimal prevAvgDays = Math.Round((decimal)TimeSpan.FromTicks((long)prevAvgTicks).TotalDays, 1);

			return new OrderDashboardKpiDto
			{
				NetRevenue = revenue,
				NetRevenueGrowthRate = CalculateGrowthRate(revenue, prevRevenue),
				PendingShipmentCount = data.PendingShipmentCount,
				PendingRefundCount = data.PendingRefundCount,
				LowStockProductCount = data.LowStockProductCount,
				ItemsPerOrder = itemsPerOrder,
				ItemsPerOrderGrowthRate = CalculateGrowthRate(itemsPerOrder, prevItemsPerOrder),
				ReturnRate = returnRate,
				ReturnRateGrowthRate = CalculateGrowthRate(returnRate, prevReturnRate),

				// 核心概覽 (新增)
				AverageOrderValue = aov,
				AovGrowthRate = CalculateGrowthRate(aov, prevAov),

				// 顧客行為 (新增)
				NewMemberCount = data.NewMemberCount,
				MemberGrowthRate = CalculateGrowthRate(data.NewMemberCount, data.PrevNewMemberCount),
				RepeatPurchaseRate = repeatRate,
				RepeatGrowthRate = CalculateGrowthRate(repeatRate, prevRepeatRate),
				ActiveMemberCount = data.UniqueMemberCount,
				ActiveGrowthRate = CalculateGrowthRate(data.UniqueMemberCount, data.PrevUniqueMemberCount),

				// 營運效率 (新增)
				AvgFulfillmentDays = curAvgDays,
				FulfillmentGrowthRate = CalculateGrowthRate(curAvgDays, prevAvgDays),
				StockAlertCount = data.LowStockProductCount
			};
		}

		public async Task<ApexChartDataDto> GetCategoryCompositionChartAsync(int? storeId, string period, string chartType)
		{
			var (start, end, _, _) = ParsePeriod(period);

			if (chartType == "Pie")
			{
				return await _orderRepository.GetCategoryCompositionPieChartAsync(storeId, start, end);
			}
			return await _orderRepository.GetCategoryCompositionBarChartAsync(storeId, start, end);
		}

		public async Task<ApexChartDataDto> GetMonthlySalesTrendAsync(int? storeId, int? year)
		{
			// 需求：抓取最近一年的有數據月份 (Rolling 12 Months)
			var now = DateTime.Now;
			var endDate = now;
			var startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-11); // 從 11 個月前的 1 號開始

			return await _orderRepository.GetMonthlySalesTrendAsync(storeId, startDate, endDate);
		}

		public async Task<List<TopProductSalesDto>> GetTopSellingCategoriesAsync(int? storeId, string period, string orderBy)
		{
			var (start, end, _, _) = ParsePeriod(period);
			return await _orderRepository.GetTopSellingCategoriesAsync(storeId, start, end, orderBy);
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

		public async Task<List<CategoryMonthlyGrowthDto>> GetCategoryMonthlyGrowthAsync(int? storeId, int year1, int year2)
		{
			var start1 = new DateTime(year1, 1, 1);
			var end1 = new DateTime(year1, 12, 31, 23, 59, 59);
			var start2 = new DateTime(year2, 1, 1);
			var end2 = new DateTime(year2, 12, 31, 23, 59, 59);

			var data1 = await _orderRepository.GetCategoryMonthlyRevenueAsync(storeId, start1, end1);
			var data2 = await _orderRepository.GetCategoryMonthlyRevenueAsync(storeId, start2, end2);

			var allCategories = data1.Select(d => d.CategoryName).Union(data2.Select(d => d.CategoryName)).Distinct().ToList();
			
			// 先計算每個月的總額，用於計算佔比
			var totals1 = new decimal[13];
			var totals2 = new decimal[13];
			for (int m = 1; m <= 12; m++)
			{
				totals1[m] = data1.Where(d => d.Month == m).Sum(d => d.Revenue);
				totals2[m] = data2.Where(d => d.Month == m).Sum(d => d.Revenue);
			}

			var result = new List<CategoryMonthlyGrowthDto>();

			foreach (var cat in allCategories)
			{
				var dto = new CategoryMonthlyGrowthDto { CategoryName = cat };
				for (int m = 1; m <= 12; m++)
				{
					// 未來月份保護
					if (year1 == DateTime.Now.Year && m > DateTime.Now.Month)
					{
						dto.MonthlyGrowthRates.Add(0);
						dto.MonthlyRevenueDeltas.Add(0);
						continue;
					}

					decimal v1 = data1.Where(d => d.CategoryName == cat && d.Month == m).Sum(d => d.Revenue);
					decimal v2 = data2.Where(d => d.CategoryName == cat && d.Month == m).Sum(d => d.Revenue);

					double share1 = totals1[m] > 0 ? (double)(v1 / totals1[m] * 100) : 0;
					double share2 = totals2[m] > 0 ? (double)(v2 / totals2[m] * 100) : 0;

					// 計算市佔率的「百分點」變動 (Percentage Point Change)
					// 例如從 10% 變成 12%，變動為 +2
					double deltaPoint = share1 - share2;
					dto.MonthlyGrowthRates.Add(Math.Round(deltaPoint, 2));
					
					// 新增：紀錄該類別在該月份的營收變動絕對值 (Year1 - Year2)
					dto.MonthlyRevenueDeltas.Add(v1 - v2);
				}
				result.Add(dto);
			}

			return result;
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
					// 未來月份保護：如果 Year1 是今年且月份大於今天，則不計算差額 (設為 0)
					if (year1 == DateTime.Now.Year && m > DateTime.Now.Month)
					{
						dto.MonthlyDeltas.Add(0);
						continue;
					}

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
