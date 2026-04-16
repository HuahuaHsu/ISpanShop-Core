using Microsoft.AspNetCore.Mvc;
using ISpanShop.Services.Coupons;
using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ISpanShop.WebAPI.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "FrontendJwt")]
    [Route("api/coupon")]
    public class CouponController : ControllerBase
    {
        private readonly ICouponService _couponService;
        private readonly ISpanShopDBContext _context;

        public CouponController(ICouponService couponService, ISpanShopDBContext context)
        {
            _couponService = couponService;
            _context = context;
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableCoupons([FromQuery] int storeId, [FromQuery] decimal subtotal, [FromQuery] string productIds)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var pIds = productIds?.Split(',').Select(int.Parse).ToList() ?? new List<int>();
            
            var coupons = await _couponService.GetAvailableCouponsAsync(userId, storeId, subtotal, pIds);
            
            return Ok(coupons.Select(c => new {
                c.Id,
                c.Title,
                c.CouponCode,
                c.CouponType,
                c.DiscountValue,
                c.MinimumSpend,
                c.MaximumDiscount,
                c.StartTime,
                c.EndTime,
                c.ApplyToAll
            }));
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMyCoupons()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var coupons = await _context.MemberCoupons
                .Include(mc => mc.Coupon)
                .Where(mc => mc.UserId == userId)
                .Select(mc => new {
                    mc.CouponId,
                    mc.Coupon.Title,
                    mc.Coupon.CouponCode,
                    mc.UsageStatus,
                    mc.Coupon.StartTime,
                    mc.Coupon.EndTime,
                    mc.Coupon.DiscountValue,
                    mc.Coupon.CouponType
                })
                .ToListAsync();

            return Ok(coupons);
        }

        [AllowAnonymous]
        [HttpGet("public-list")]
        public async Task<IActionResult> GetPublicCoupons()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = string.IsNullOrEmpty(userIdStr) ? null : int.Parse(userIdStr);

                var coupons = await _couponService.GetPublicCouponsAsync(userId);
                
                var claimedIds = new List<int>();
                if (userId.HasValue)
                {
                    claimedIds = await _context.MemberCoupons
                        .Where(mc => mc.UserId == userId.Value)
                        .Select(mc => mc.CouponId)
                        .ToListAsync();
                }

                var result = coupons.Select(c => new {
                    c.Id,
                    c.Title,
                    c.CouponCode,
                    c.CouponType,
                    c.DiscountValue,
                    c.MinimumSpend,
                    c.StartTime,
                    c.EndTime,
                    c.TotalQuantity,
                    c.UsedQuantity,
                    c.PerUserLimit,
                    IsClaimed = claimedIds.Contains(c.Id)
                });

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "獲取優惠券清單失敗: " + ex.Message });
            }
        }

        [HttpPost("claim/{id}")]
        public async Task<IActionResult> ClaimCoupon(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized(new { message = "請先登入後再領取優惠券" });
            int userId = int.Parse(userIdStr);

            var (success, message) = await _couponService.ClaimCouponAsync(userId, id);

            if (!success) return BadRequest(new { message });

            return Ok(new { message });
        }

        [AllowAnonymous]
        [HttpGet("fix-locks")]
        public async Task<IActionResult> FixLocks()
        {
            int count = await _couponService.ForceReleaseAllExpiredLocksAsync();
            return Ok(new { message = $"修復完成，已釋放 {count} 筆異常鎖定的優惠券" });
        }
    }
}
