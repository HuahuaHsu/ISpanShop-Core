using ISpanShop.Models.EfModels;
using ISpanShop.Models.EfModels.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Repositories.Interfaces
{
	public interface IOrderRepository
	{
		//取得訂單列表、取得單筆訂單詳細資訊、更新訂單狀態等。
		Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedOrdersAsync(OrderSearchDto search);

		/// <summary>
		/// 取得單筆訂單完整資訊（含明細與關聯資料）
		/// </summary>
		Task<Order> GetOrderByIdAsync(long id);

		/// <summary>
		/// 更新訂單狀態
		/// </summary>
		Task UpdateStatusAsync(long id, byte status);

	}
}
