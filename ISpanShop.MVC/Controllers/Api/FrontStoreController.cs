using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ISpanShop.Services.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Controllers.Api
{
    [ApiController]
    [Route("api/front/[controller]")]
    [Authorize(AuthenticationSchemes = "FrontendJwt")]
    public class StoreController : ControllerBase
    {
        private readonly IFrontStoreService _storeService;

        public StoreController(IFrontStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized();
                }

                var dashboardData = await _storeService.GetDashboardDataAsync(userId);
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
