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

		public CheckoutController(CheckoutService checkoutService)
		{
			_checkoutService = checkoutService;
		}

		[HttpPost]
		public async Task<IActionResult> CreateOrder([FromBody] CheckoutRequestDTO dto)
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

			return Ok(new
			{
				success = true,
				message = result.Message,
				orderNumber = result.OrderNumber,
				paymentUrl = "/Payment/Pay?orderNumber=" + result.OrderNumber
			});
		}
	}
}
