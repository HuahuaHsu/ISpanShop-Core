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
		Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedOrdersAsync(OrderSearchDto search);
	}
}
