using ISpanShop.Models.DTOs;
using ISpanShop.Models.DTOs.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Services.Orders
{
	public interface IOrderDashboardService
	{
		/// <summary>
		/// 取得儀表板資訊小卡 (包含趨勢對比)
		/// period 參數可為 "day", "week", "month", "7days", "3months" 等
		/// </summary>
		Task<OrderDashboardKpiDto> GetDashboardKpisAsync(int? storeId, string period);

		Task<ApexChartDataDto> GetCategoryCompositionChartAsync(int? storeId, string period, string chartType); // chartType: Bar or Pie
		Task<ApexChartDataDto> GetMonthlySalesTrendAsync(int? storeId, int? year);
		Task<ApexChartDataDto> GetCategoryContributionAsync(int? storeId, string period);
		Task<ApexChartDataDto> GetCategoryContributionAsync(int? storeId, DateTime start, DateTime end);
		Task<List<TopProductSalesDto>> GetTopSellingCategoriesAsync(int? storeId, string period, string orderBy);
		Task<ApexChartDataDto> GetYearlyRevenueDataAsync(int? storeId, int year);
		Task<ApexChartDataDto> GetCategoryDetailAsync(int? storeId, string period, string type, string categoryName);
		Task<List<CategoryMonthlyDeltaDto>> GetCategoryMonthlyDeltaAsync(int? storeId, int year1, int year2);
		Task<List<CategoryMonthlyGrowthDto>> GetCategoryMonthlyGrowthAsync(int? storeId, int year1, int year2);
	}

	public class CategoryMonthlyDeltaDto
	{
		public string CategoryName { get; set; }
		public List<decimal> MonthlyDeltas { get; set; } = new List<decimal>(); // 12個月的差額
	}

	public class CategoryMonthlyGrowthDto
	{
		public string CategoryName { get; set; }
		public List<double> MonthlyGrowthRates { get; set; } = new List<double>(); // 12個月的增長率 (百分比數值，如 50.5 代表 50.5%)
		public List<decimal> MonthlyRevenueDeltas { get; set; } = new List<decimal>(); // 12個月的營收變動額
	}
}

