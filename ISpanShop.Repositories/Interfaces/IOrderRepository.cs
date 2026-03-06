using ISpanShop.Models.DTOs;
using ISpanShop.Models.EfModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Repositories.Interfaces
{
	public interface IOrderRepository
	{
		// 取得單筆訂單完整資訊（含明細與關聯資料）
		Task<Order> GetOrderByIdAsync(long id);

		// 更新訂單狀態
		Task UpdateStatusAsync(long id, byte status);

		// 取得分頁與篩選後的訂單清單 (主要用於列表頁 A & B 需求)
		Task<PagedResultDto<OrderListDto>> GetFilteredOrdersAsync(OrderSearchDto criteria);

		// 儀表板數據查詢 (C & D 需求)
		Task<DashboardKpiRawDataDto> GetDashboardKpisAsync(int? storeId, DateTime startDate, DateTime endDate, DateTime prevStartDate, DateTime prevEndDate);
		Task<ApexChartDataDto> GetProductSalesBarChartAsync(int? storeId, DateTime startDate, DateTime endDate);
		Task<ApexChartDataDto> GetProductSalesPieChartAsync(int? storeId, DateTime startDate, DateTime endDate);
		Task<ApexChartDataDto> GetMonthlySalesTrendAsync(int? storeId, DateTime startDate, DateTime endDate);
		Task<List<TopProductSalesDto>> GetTop10ProductsAsync(int? storeId, DateTime startDate, DateTime endDate, string orderBy);
		Task<ApexChartDataDto> GetCategoryContributionAsync(int? storeId, DateTime startDate, DateTime endDate);
	}
}
