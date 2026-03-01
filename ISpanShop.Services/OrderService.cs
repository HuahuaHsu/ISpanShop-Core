using ISpanShop.Common.Enums;
using ISpanShop.Models.EfModels.DTOs;
using ISpanShop.Repositories.Interfaces;
using ISpanShop.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Services
{
	public class OrderService : IOrderService
	{
		private readonly IOrderRepository _orderRepository;

		public OrderService(IOrderRepository orderRepository)
		{
			_orderRepository = orderRepository;
		}

		public async Task<PagedResultDto<OrderDto>> GetOrdersAsync(OrderSearchDto search)
		{
			var (entities, totalCount) = await _orderRepository.GetPagedOrdersAsync(search);

			// Entity 轉換為 DTO
			var dtos = entities.Select(o => new OrderDto
			{
				Id = o.Id,
				OrderNumber = o.OrderNumber,
				UserId = o.UserId,
				UserName = o.User?.Account ?? "未知用戶",
				StoreId = o.StoreId,
				StoreName = o.Store?.StoreName ?? "未知商店",
				TotalAmount = o.TotalAmount,
				ShippingFee = o.ShippingFee,
				FinalAmount = o.FinalAmount,
				Status = (OrderStatus)(o.Status ?? 0),
				RecipientName = o.RecipientName,
				RecipientPhone = o.RecipientPhone,
				RecipientAddress = o.RecipientAddress,
				CreatedAt = o.CreatedAt
			}).ToList();

			return new PagedResultDto<OrderDto>
			{
				Items = dtos,
				TotalCount = totalCount,
				PageNumber = search.PageNumber,
				PageSize = search.PageSize
			};
		}
	}
}
