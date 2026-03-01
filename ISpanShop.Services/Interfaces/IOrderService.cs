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
	}
}
