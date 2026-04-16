using Microsoft.AspNetCore.Mvc;
using ISpanShop.Services.Orders;
using ISpanShop.Common.Enums;
using ISpanShop.Common.Helpers;
using ISpanShop.Models.DTOs.Orders;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ISpanShop.MVC.Controllers.Api.Orders
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[Route("api/orders")]
	[ApiController]
	public class OrdersApiController : ControllerBase
	{
		private readonly IOrderService _orderService;

		public OrdersApiController(IOrderService orderService)
		{
			_orderService = orderService;
		}

		// GET: api/orders?pageNumber=1&pageSize=10&statuses=0&statuses=1
		[HttpGet]
		public async Task<IActionResult> GetMyOrders([FromQuery] List<int> statuses, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
		{
			var userId = User.GetUserId();
			if (userId == null) return Unauthorized();

			// 封裝查詢條件
			var criteria = new OrderSearchDto
			{
				UserId = userId.Value,
				Statuses = statuses,
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
			var userId = User.GetUserId();
			if (userId == null) return Unauthorized();

			var order = await _orderService.GetOrderDetailAsync(id);

			if (order == null)
			{
				return NotFound(new { message = "找不到該訂單" });
			}

			// 權限檢查：確保只能讀取自己的訂單
			if (order.UserId != userId.Value)
			{
				return Forbid();
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

		// POST: api/orders/test-generate
		[HttpPost("test-generate")]
		public async Task<IActionResult> CreateTestOrder()
		{
			try
			{
				var userId = User.GetUserId();
				if (userId == null) return Unauthorized();

				var dbContext = (ISpanShop.Models.EfModels.ISpanShopDBContext)HttpContext.RequestServices.GetService(typeof(ISpanShop.Models.EfModels.ISpanShopDBContext));
				
				// 取得目前時間與單號
				var now = DateTime.Now;
				var orderNum = "TEST-" + now.ToString("yyyyMMddHHmmss");

				// 1. 確保至少有一間商店 (直接用 SQL 查，沒有就生)
				var storeId = await dbContext.Database.ExecuteSqlRawAsync(
					"IF NOT EXISTS (SELECT 1 FROM Stores) INSERT INTO Stores (StoreName) VALUES (N'測試自動商店')");
				
				var actualStoreId = (await dbContext.Stores.AsNoTracking().FirstOrDefaultAsync())?.Id ?? 1;

				// 2. 直接用 SQL 寫入 Order 表 (避免導航屬性導致的 500)
				await dbContext.Database.ExecuteSqlInterpolatedAsync(@$"
					INSERT INTO Orders (OrderNumber, UserId, StoreId, TotalAmount, ShippingFee, FinalAmount, Status, CreatedAt, RecipientName, RecipientPhone, RecipientAddress)
					VALUES ({orderNum}, {userId.Value}, {actualStoreId}, 1500, 60, 1560, 3, {now}, N'測試人', '0912345678', N'測試地址')");

				// 3. 取得剛產生的 OrderId
				var orderId = (await dbContext.Orders.AsNoTracking().OrderByDescending(o => o.Id).FirstOrDefaultAsync())?.Id ?? 0;

				// 4. 寫入一筆明細
				await dbContext.Database.ExecuteSqlInterpolatedAsync(@$"
					INSERT INTO OrderDetails (OrderId, ProductId, ProductName, Price, Quantity)
					VALUES ({orderId}, 1, N'測試假商品', 1500, 1)");

				return Ok(new { message = "測試資料已用原始 SQL 強制寫入（狀態：已完成）", orderId = orderId });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "強制生成失敗", detail = ex.Message + " " + ex.InnerException?.Message });
			}
		}

		// POST: api/orders/1/cancel
		[HttpPost("{id}/cancel")]
		public async Task<IActionResult> CancelOrder(long id)
		{
			var userId = User.GetUserId();
			if (userId == null) return Unauthorized();

			var order = await _orderService.GetOrderDetailAsync(id);
			if (order == null) return NotFound(new { message = "找不到該訂單" });
			if (order.UserId != userId.Value) return Forbid();

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
			var userId = User.GetUserId();
			if (userId == null) return Unauthorized();

			var order = await _orderService.GetOrderDetailAsync(id);
			if (order == null) return NotFound(new { message = "找不到該訂單" });
			if (order.UserId != userId.Value) return Forbid();

			var success = await _orderService.ReturnOrderAsync(id);
			if (success)
			{
				return Ok(new { message = "已提交退貨申請" });
			}
			return BadRequest(new { message = "無法提交退貨申請，可能狀態不符或訂單不存在" });
		}
	}
}