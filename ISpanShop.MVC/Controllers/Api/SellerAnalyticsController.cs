using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ISpanShop.Services.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISpanShop.Services.Members;

namespace ISpanShop.MVC.Controllers.Api
{
    [ApiController]
    [Route("api/front/seller/analytics")]
    [Authorize(AuthenticationSchemes = "FrontendJwt")]
    public class SellerAnalyticsController : ControllerBase
    {
        private readonly IStoreService _storeService;
        private readonly IFrontStoreService _frontStoreService;

        public SellerAnalyticsController(IStoreService storeService, IFrontStoreService frontStoreService)
        {
            _storeService = storeService;
            _frontStoreService = frontStoreService;
        }

        [HttpGet("traffic")]
        public async Task<IActionResult> GetTrafficAnalytics()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            // 先取得賣家的 StoreId
            var store = await _frontStoreService.GetStoreByUserIdAsync(userId);
            if (store == null)
            {
                return NotFound(new { message = "找不到賣場資訊" });
            }

            var analytics = await _storeService.GetTrafficAnalyticsAsync(store.Id);
            return Ok(analytics);
        }
    }
}
