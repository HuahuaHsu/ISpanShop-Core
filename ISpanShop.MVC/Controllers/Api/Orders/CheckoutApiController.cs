using Microsoft.AspNetCore.Mvc;
using ISpanShop.Services.Payments;
using ISpanShop.Models.DTOs.Orders;
using ISpanShop.Services;
using Microsoft.AspNetCore.Authorization;
using ISpanShop.Common.Helpers;

using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ISpanShop.MVC.Controllers.Api.Orders
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[ApiController]
	[Route("api/checkout")]
	public class CheckoutApiController : ControllerBase
	{
		private readonly CheckoutService _checkoutService;
		private readonly PaymentService _paymentService;

		public CheckoutApiController(CheckoutService checkoutService, PaymentService paymentService)
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
				return BadRequest(new { message = "購物車內容不可為空" });
			}

			// 權限檢查：強迫從 Token 取得 UserId，防止前端惡意修改 UserId
			var userId = User.GetUserId();
			if (userId == null) return Unauthorized();

			dto.UserId = userId.Value;

			// 1. 呼叫 Service 建立訂單並處理點數
			var result = await _checkoutService.CreateOrderAsync(dto);

			if (!result.IsSuccess)
			{
				return BadRequest(new { message = result.Message });
			}

			// 2. 訂單建立成功，回傳結帳所需的資訊
			return Ok(new
			{
				success = true,
				message = result.Message,
				orderNumber = result.OrderNumber,
				// 導向付款頁面（此處可依需求調整導向綠界或自訂付款頁）
				paymentUrl = "/api/Payment/GoToEcpay?orderNumber=" + result.OrderNumber
			});
		}
	}
}
