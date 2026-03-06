using ISpanShop.Common.Enums;
using ISpanShop.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Services.Interfaces
{
	public interface IOrderService
	{
		/// <summary>
		/// 取得分頁與篩選後的訂單列表 (A & B 需求)
		/// </summary>
		Task<PagedResultDto<OrderListDto>> GetFilteredOrdersAsync(OrderSearchDto criteria);

		/// <summary>
		/// 取得訂單明細資訊
		/// </summary>
		Task<OrderFullDto> GetOrderDetailAsync(long id);

		/// <summary>
		/// 更新訂單狀態業務邏輯
		/// </summary>
		Task UpdateStatusAsync(long id, OrderStatus status);
	}
}
