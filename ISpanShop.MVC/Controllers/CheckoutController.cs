using Microsoft.AspNetCore.Mvc;
using ISpanShop.Services;
using ISpanShop.Models.DTOs;

namespace ISpanShop.WebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
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

			// 1. 呼叫 Service 建立訂單並處理點數
			var result = await _checkoutService.CreateOrderAsync(dto);

			if (!result.IsSuccess)
			{
				return BadRequest(new { message = result.Message });
			}

			// 2. 訂單建立成功，準備回傳給前端所需的資訊
			// 在實際金流串接中，這裡通常會回傳一個自動導向綠界的 HTML Form 或是 URL
			return Ok(new
			{
				success = true,
				message = result.Message,
				orderNumber = result.OrderNumber,
				// 這裡可以預留呼叫綠界產生付款 Form 的邏輯
				paymentUrl = "/api/Payment/GoToEcpay?orderNumber=" + result.OrderNumber
			});
		}
	}
}