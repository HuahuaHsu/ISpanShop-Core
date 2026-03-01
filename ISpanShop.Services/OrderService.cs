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
			// 可以在此加入額外的商務檢核（例如：已取消的訂單不能再改為已出貨）
			await _orderRepository.UpdateStatusAsync(id, (byte)status);
		}
	}
}
