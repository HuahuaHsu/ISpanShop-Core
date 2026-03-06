using ISpanShop.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Services.Interfaces
{
	public interface IOrderDashboardService
	{
		/// <summary>
		/// 取得儀表板資訊小卡 (包含趨勢對比)
		/// period 參數可為 "day", "week", "month", "7days", "3months" 等
		/// </summary>
		Task<OrderDashboardKpiDto> GetDashboardKpisAsync(int? storeId, string period);

		Task<ApexChartDataDto> GetProductSalesChartAsync(int? storeId, string period, string chartType); // chartType: Bar or Pie
		Task<ApexChartDataDto> GetMonthlySalesTrendAsync(int? storeId, int? year);
		Task<ApexChartDataDto> GetCategoryContributionAsync(int? storeId, string period);
		Task<List<TopProductSalesDto>> GetTop10ProductsAsync(int? storeId, string period, string orderBy);
	}
}

