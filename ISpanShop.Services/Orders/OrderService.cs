using ISpanShop.Common.Enums;
using ISpanShop.Models.DTOs.Orders;
using ISpanShop.Repositories.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Services.Orders
{
	public class OrderService : IOrderService
	{
		private readonly IOrderRepository _orderRepository;

		public OrderService(IOrderRepository orderRepository)
		{
			_orderRepository = orderRepository;
		}

		public async Task<IDictionary<byte, int>> GetOrderStatusCountsAsync()
		{
			return await _orderRepository.GetOrderStatusCountsAsync();
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
				ReturnRequestImages = o.ReturnRequests
					.SelectMany(rr => rr.ReturnRequestImages)
					.Select(rri => rri.ImageUrl)
					.ToList(),
				ReturnReason = o.ReturnRequests.OrderByDescending(r => r.CreatedAt).FirstOrDefault()?.ReasonCategory,
				ReturnDescription = o.ReturnRequests.OrderByDescending(r => r.CreatedAt).FirstOrDefault()?.ReasonDescription,
				ReturnRequestCreatedAt = o.ReturnRequests.OrderByDescending(r => r.CreatedAt).FirstOrDefault()?.CreatedAt,
				RefundDate = o.ReturnRequests.Where(r => r.Status == 1).OrderByDescending(r => r.UpdatedAt).FirstOrDefault()?.UpdatedAt,
				Details = o.OrderDetails.Select(od => new OrderDetailDto
				{
					Id = od.Id,
					ProductId = od.ProductId,
					VariantId = od.VariantId,
					ProductName = od.ProductName,
					VariantName = od.VariantName,
					SkuCode = od.SkuCode,
					CoverImage = od.CoverImage,
					Price = od.Price ?? 0,
					Quantity = od.Quantity,
					Stock = od.Product?.ProductVariants?.FirstOrDefault(v => v.Id == od.VariantId)?.Stock ?? 0
				}).ToList()
			};
		}

		public async Task UpdateStatusAsync(long id, OrderStatus status)
		{
			await _orderRepository.UpdateStatusAsync(id, (byte)status);
		}
	}
}
