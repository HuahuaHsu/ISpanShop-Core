using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Orders;
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

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(long id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var success = await _orderService.CancelOrderAsync(id, userId);
            if (!success)
            {
                return BadRequest(new { message = "訂單無法取消（可能狀態不符或非本人訂單）" });
            }

            return Ok(new { message = "訂單已成功取消" });
        }

        [HttpPost("{id}/confirm-receipt")]
        public async Task<IActionResult> ConfirmReceipt(long id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var success = await _orderService.ConfirmReceiptAsync(id, userId);
            if (!success)
            {
                return BadRequest(new { message = "無法確認收貨（可能狀態不符或非本人訂單）" });
            }

            return Ok(new { message = "訂單已完成" });
        }

        [HttpPost("{id}/return")]
        public async Task<IActionResult> RequestReturn(long id, [FromBody] FrontReturnRequestDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var success = await _orderService.RequestReturnAsync(id, userId, dto);
            if (!success)
            {
                return BadRequest(new { message = "無法發起退貨申請（可能狀態不符或非本人訂單）" });
            }

            return Ok(new { message = "退貨申請已送出，請等待賣家處理" });
        }
    }
}
