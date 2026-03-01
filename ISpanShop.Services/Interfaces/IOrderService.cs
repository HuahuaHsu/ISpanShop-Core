using ISpanShop.Common.Enums;
using ISpanShop.Models.EfModels.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Services.Interfaces
{
	public interface IOrderService
	{
		Task<PagedResultDto<OrderDto>> GetOrdersAsync(OrderSearchDto search);

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
