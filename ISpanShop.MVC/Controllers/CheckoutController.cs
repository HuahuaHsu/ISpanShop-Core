using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ISpanShop.Services.Payments;
using ISpanShop.Models.DTOs.Orders;
using ISpanShop.Services;

namespace ISpanShop.WebAPI.Controllers
{
	[ApiController]
	[Authorize]
	[Route("api/checkout")]
	public class CheckoutController : ControllerBase
	{
		private readonly CheckoutService _checkoutService;
		private readonly PaymentService _paymentService;

		public CheckoutController(CheckoutService checkoutService, PaymentService paymentService)
		{
			_checkoutService = checkoutService;
			_paymentService = paymentService;
		}

		/// <summary>
		/// 接收前端結帳請求
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> CreateOrder([FromBody] CheckoutRequestDTO dto)
		{
			if (dto == null || dto.Items == null || !dto.Items.Any())
			{
				return BadRequest("購物車內容不可為空");
			}

			// 強制從登入資訊獲取 UserId
			var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
			dto.UserId = int.Parse(userIdStr);

			// 1. 呼叫 Service 建立訂單並處理點數.
			var result = await _checkoutService.CreateOrderAsync(dto);

			if (!result.IsSuccess)
			{
				return BadRequest(new { message = result.Message });
			}
	// ... (rest of code)
			// 2. 訂單建立成功，準備回傳給前端所需的資訊
			return Ok(new
			{
				success = true,
				message = result.Message,
				orderNumber = result.OrderNumber,
				// 修正：指向 Payment 控制器的 Pay 方法
				paymentUrl = "/Payment/Pay?orderNumber=" + result.OrderNumber
			});
		}
	}
}