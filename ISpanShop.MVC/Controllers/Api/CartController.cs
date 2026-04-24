using System;
using System.Collections.Generic;
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
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var cart = await _cartService.GetCartAsync(userId);
            return Ok(cart);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequestDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var success = await _cartService.AddToCartAsync(userId, dto);
            if (!success) return BadRequest(new { message = "加入購物車失敗" });

            return Ok(new { message = "已加入購物車" });
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemRequestDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var success = await _cartService.UpdateCartItemAsync(userId, dto);
            if (!success) return BadRequest(new { message = "更新失敗" });

            return Ok(new { message = "更新成功" });
        }

        [HttpDelete("remove/{productId}/{variantId?}")]
        public async Task<IActionResult> RemoveCartItem(int productId, int? variantId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var success = await _cartService.RemoveCartItemAsync(userId, productId, variantId);
            if (!success) return BadRequest(new { message = "移除失敗" });

            return Ok(new { message = "已從購物車移除" });
        }

        [HttpPost("sync")]
        public async Task<IActionResult> SyncCart([FromBody] List<CartItemDto> localItems)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            await _cartService.SyncCartAsync(userId, localItems);
            var updatedCart = await _cartService.GetCartAsync(userId);
            return Ok(updatedCart);
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            await _cartService.ClearCartAsync(userId);
            return Ok(new { message = "購物車已清空" });
        }
    }
}
