using Microsoft.AspNetCore.Mvc;
using ISpanShop.Services.Orders;
using ISpanShop.Common.Enums;
using ISpanShop.Models.DTOs.Orders;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace ISpanShop.MVC.Controllers.Api.Orders
{
	[Route("api/orders")]
	[ApiController]
	public class OrdersApiController : ControllerBase
	{
		private readonly IOrderService _orderService;

		public OrdersApiController(IOrderService orderService)
		{
			_orderService = orderService;
		}

		// GET: api/orders?userId=1&pageNumber=1&pageSize=10
		[HttpGet]
		public async Task<IActionResult> GetMyOrders([FromQuery] int userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
		{
			// 封裝查詢條件
			var criteria = new OrderSearchDto
			{
				UserId = userId,
				PageNumber = pageNumber,
				PageSize = pageSize,
				IsDescending = true, // 最新訂單排在前面
				SortBy = "CreatedAt"
			};

			var result = await _orderService.GetFilteredOrdersAsync(criteria);
			
			// 轉換為前端較易讀的 camelCase 格式
			return Ok(new
			{
				items = result.Items.Select(o => new
				{
					id = o.OrderId,
					orderNumber = o.OrderUuid,
					totalAmount = o.TotalAmount,
					status = o.StatusId,
					statusName = o.StatusName,
					createdAt = o.OrderDate,
					storeName = o.StoreName
				}),
				totalCount = result.TotalCount,
				pageNumber = result.PageNumber,
				pageSize = result.PageSize
			});
		}

		// GET: api/orders/1
		[HttpGet("{id}")]
		public async Task<IActionResult> GetOrderDetail(long id)
		{
			var order = await _orderService.GetOrderDetailAsync(id);

			if (order == null)
			{
				return NotFound(new { message = "找不到該訂單" });
			}

			// 維持與前端對接的屬性名稱 (camelCase)
			var result = new
			{
				id = order.Id.ToString(),
				orderNumber = order.OrderNumber,
				receiverName = order.RecipientName,
				receiverPhone = order.RecipientPhone,
				receiverAddress = order.RecipientAddress,
				finalAmount = order.FinalAmount,
				status = (byte)order.Status,
				createdAt = order.CreatedAt,
				items = order.Details.Select(d => new {
					id = d.Id,
					productId = d.ProductId,
					productName = d.ProductName ?? "未知商品",
					variantName = d.VariantName ?? "預設規格",
					imageUrl = d.CoverImage ?? "/images/no-image.png",
					unitPrice = d.Price,
					quantity = d.Quantity,
					subTotal = d.Price * d.Quantity
				}).ToList()
			};

			return Ok(result);
		}

		// POST: api/orders/1/cancel
		[HttpPost("{id}/cancel")]
		public async Task<IActionResult> CancelOrder(long id)
		{
			var success = await _orderService.CancelOrderAsync(id);
			if (success)
			{
				return Ok(new { message = "訂單已取消" });
			}
			return BadRequest(new { message = "無法取消該訂單，可能狀態不符或訂單不存在" });
		}

		// POST: api/orders/1/return
		[HttpPost("{id}/return")]
		public async Task<IActionResult> ReturnOrder(long id)
		{
			var success = await _orderService.ReturnOrderAsync(id);
			if (success)
			{
				return Ok(new { message = "已提交退貨申請" });
			}
			return BadRequest(new { message = "無法提交退貨申請，可能狀態不符或訂單不存在" });
		}
	}
}