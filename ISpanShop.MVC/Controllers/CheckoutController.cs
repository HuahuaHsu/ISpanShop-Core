using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ISpanShop.Services.Payments;
using ISpanShop.Models.DTOs.Orders;
using ISpanShop.Services;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;

namespace ISpanShop.WebAPI.Controllers
{
	[ApiController]
	[Authorize(AuthenticationSchemes = "FrontendJwt")]
	[Route("api/checkout")]
	public class CheckoutController : ControllerBase
	{
		private readonly CheckoutService _checkoutService;
		private readonly ISpanShop.Services.Orders.IFrontOrderService _orderService;

		public CheckoutController(CheckoutService checkoutService, ISpanShop.Services.Orders.IFrontOrderService orderService)
		{
			_checkoutService = checkoutService;
			_orderService = orderService;
		}

		[HttpGet("repay/{orderId}")]
		public async Task<IActionResult> GetRepaymentUrl(long orderId)
		{
			// 1. 強制身分驗證
			var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
				return Unauthorized();

			// 2. 獲取訂單並檢查權限與狀態
			var order = await _orderService.GetOrderDetailAsync(orderId, userId);
			if (order == null) return NotFound(new { message = "找不到訂單" });
			
			if ((int)order.Status != 0) // 只有待付款能再次獲取支付連結
				return BadRequest(new { message = "該訂單目前的狀態無法進行支付" });

			// 3. 根據支付方式產生跳轉路徑 (目前預設 ECPay，可擴充)
			// 這裡預留邏輯，未來可根據 order.PaymentMethod 分流
			string paymentUrl = "/Payment/Pay?orderNumber=" + order.OrderNumber;

			return Ok(new
			{
				success = true,
				paymentUrl = paymentUrl
			});
		}

		[HttpPost]
		public async Task<IActionResult> CreateOrder([FromBody] CheckoutRequestDto dto)
		{
			if (dto == null || dto.Items == null || !dto.Items.Any())
			{
				return BadRequest(new { message = "購物車內容不可為空" });
			}

			// 強制從 JWT 獲取正確的 UserId
			var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdStr)) return Unauthorized(new { message = "無法識別使用者身分" });
			dto.UserId = int.Parse(userIdStr);

			var result = await _checkoutService.CreateOrderAsync(dto);

			if (!result.IsSuccess)
			{
				return BadRequest(new { message = result.Message });
			}

			string paymentUrl = dto.PaymentMethod == "NewebPay"
				? "/PaymentNewebPay/Pay?orderNumber=" + result.OrderNumber
				: "/Payment/Pay?orderNumber=" + result.OrderNumber;

			return Ok(new
			{
				success = true,
				message = result.Message,
				orderNumber = result.OrderNumber,
				paymentUrl = paymentUrl
			});
		}
	}
}
