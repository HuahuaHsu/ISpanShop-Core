using ISpanShop.Common.Enums;
using ISpanShop.Models.DTOs;
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

		public async Task<PagedResultDto<OrderListDto>> GetFilteredOrdersAsync(OrderSearchDto criteria)
		{
			return await _orderRepository.GetFilteredOrdersAsync(criteria);
		}

		public async Task<OrderFullDto> GetOrderDetailAsync(long id)
		{
			var o = await _orderRepository.GetOrderByIdAsync(id);
			if (o == null) return null;

			return new OrderFullDto
			{
				Id = o.Id,
				OrderNumber = o.OrderNumber,
				UserId = o.UserId,
				UserName = o.User?.Account,
				StoreId = o.StoreId,
				StoreName = o.Store?.StoreName,
				TotalAmount = o.TotalAmount,
				ShippingFee = o.ShippingFee,
				FinalAmount = o.FinalAmount,
				Status = (OrderStatus)(o.Status ?? 0),
				RecipientName = o.RecipientName,
				RecipientPhone = o.RecipientPhone,
				RecipientAddress = o.RecipientAddress,
				CreatedAt = o.CreatedAt,
				PaymentDate = o.PaymentDate,
				CompletedAt = o.CompletedAt,
				Note = o.Note,
				Details = o.OrderDetails.Select(od => new OrderDetailDto
				{
					Id = od.Id,
					ProductId = od.ProductId,
					ProductName = od.ProductName,
					VariantName = od.VariantName,
					SkuCode = od.SkuCode,
					CoverImage = od.CoverImage,
					Price = od.Price ?? 0,
					Quantity = od.Quantity
				}).ToList()
			};
		}

		public async Task UpdateStatusAsync(long id, OrderStatus status)
		{
			await _orderRepository.UpdateStatusAsync(id, (byte)status);
		}
	}
}
