using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ISpanShop.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Controllers.Api
{
    [ApiController]
    [Route("api/front/[controller]")]
    [Authorize(AuthenticationSchemes = "FrontendJwt")]
    public class OrdersController : ControllerBase
    {
        private readonly IFrontOrderService _orderService;

        public OrdersController(IFrontOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var orders = await _orderService.GetMemberOrdersAsync(userId);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetail(long id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var order = await _orderService.GetOrderDetailAsync(id, userId);
            if (order == null)
            {
                return NotFound(new { message = "找不到該訂單或您無權存取" });
            }

            return Ok(order);
        }
    }
}
