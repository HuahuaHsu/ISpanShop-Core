using ISpanShop.Common.Enums;
using ISpanShop.Models.DTOs.Orders;
using ISpanShop.Models.DTOs.Members;
using ISpanShop.Repositories.Orders;
using ISpanShop.Services.Payments;
using ISpanShop.Services.Coupons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISpanShop.Repositories.Products;
using ISpanShop.Models.EfModels;

namespace ISpanShop.Services.Orders
{
	public class OrderService : IOrderService
	{
		private readonly IOrderRepository _orderRepository;
		private readonly PointService _pointService;
		private readonly ICouponService _couponService;
		private readonly IProductRepository _productRepository;

		public OrderService(IOrderRepository orderRepository, PointService pointService, ICouponService couponService, IProductRepository productRepository)
		{
			_orderRepository = orderRepository;
			_pointService = pointService;
			_couponService = couponService;
			_productRepository = productRepository;
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
				PointDiscount = o.PointDiscount,
				DiscountAmount = o.DiscountAmount,
				FinalAmount = o.FinalAmount,
				CouponId = o.CouponId,
				CouponName = o.Coupon?.Title,
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
				RefundDate = o.ReturnRequests.OrderByDescending(r => r.UpdatedAt).FirstOrDefault()?.UpdatedAt,
				Details = o.OrderDetails.Select(od => {
					// 優先順序：1. 明細快照圖 2. 變體專屬圖 3. 產品主圖 4. 產品任意第一張圖
					string finalCover = od.CoverImage;
					if (string.IsNullOrEmpty(finalCover))
					{
						var variantImage = od.Product?.ProductVariants?
							.FirstOrDefault(v => v.Id == od.VariantId)?
							.ProductImages?.FirstOrDefault()?.ImageUrl;
							
						finalCover = variantImage 
							?? od.Product?.ProductImages?.FirstOrDefault(pi => pi.IsMain == true)?.ImageUrl
							?? od.Product?.ProductImages?.FirstOrDefault()?.ImageUrl;
					}

					return new OrderDetailDto
					{
						Id = od.Id,
						ProductId = od.ProductId,
						VariantId = od.VariantId,
						ProductName = od.ProductName,
						VariantName = od.VariantName,
						SkuCode = od.SkuCode,
						CoverImage = finalCover,
						Price = od.Price ?? 0,
						Quantity = od.Quantity,
						Stock = od.Product?.ProductVariants?.FirstOrDefault(v => v.Id == od.VariantId)?.Stock ?? 0
					};
				}).ToList()
			};
		}

		public async Task UpdateStatusAsync(long id, OrderStatus status)
		{
			await _orderRepository.UpdateStatusAsync(id, (byte)status);

			if (status == OrderStatus.Completed)
			{
				var order = await _orderRepository.GetOrderByIdAsync(id);
				if (order != null)
				{
					// 依訂單最終金額 1% 贈點，最少 10 點
					int rewardPoints = Math.Max(10, (int)(order.FinalAmount * 0.01m));
					await _pointService.UpdatePointsAsync(new PointUpdateDTO
					{
						UserId = order.UserId,
						ChangeAmount = rewardPoints,
						OrderNumber = order.OrderNumber,
						Description = "訂單完成贈點"
					});
				}
			}
			else if (status == OrderStatus.Refunded)
			{
				var order = await _orderRepository.GetOrderByIdAsync(id);
				if (order != null)
				{
					// 1. 退還點數
					if (order.PointDiscount.HasValue && order.PointDiscount.Value > 0)
					{
						await _pointService.UpdatePointsAsync(new PointUpdateDTO
						{
							UserId = order.UserId,
							ChangeAmount = order.PointDiscount.Value,
							OrderNumber = order.OrderNumber,
							Description = $"訂單退款點數退還 (訂單號: {order.OrderNumber})"
						});
					}

					// 2. 退還優惠券
					if (order.CouponId.HasValue)
					{
						await _couponService.ReturnCouponAsync(order.Id);
					}

					// 3. 歸還庫存
					await ReturnStockAsync(order.OrderDetails);
				}
			}
		}

		public async Task<bool> CancelOrderAsync(long id)
		{
			var order = await _orderRepository.GetOrderByIdAsync(id);
			if (order == null) return false;

			// 僅在「待付款(0)」或「待出貨(1)」時允許取消
			if (order.Status == 0 || order.Status == 1)
			{
				await _orderRepository.UpdateStatusAsync(id, (byte)OrderStatus.Cancelled);

				// 歸還庫存
				await ReturnStockAsync(order.OrderDetails);

				return true;
			}

			return false;
		}

		private async Task ReturnStockAsync(IEnumerable<OrderDetail> details)
		{
			foreach (var detail in details)
			{
				await _productRepository.UpdateStockAsync(detail.ProductId, detail.VariantId, detail.Quantity);
			}
		}

		public async Task<bool> ReturnOrderAsync(long id)
		{
			var order = await _orderRepository.GetOrderByIdAsync(id);
			if (order == null) return false;

			// 僅在「已完成(3)」時允許申請退貨
			if (order.Status == 3)
			{
				await _orderRepository.UpdateStatusAsync(id, (byte)OrderStatus.Returning);
				return true;
			}

			return false;
		}
	}
}
