using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ISpanShop.Models.EfModels;
using ISpanShop.Services.Coupons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.MVC.Controllers.Api.Coupons
{
    [ApiController]
    [Route("api/seller/coupons")]
    [Authorize(AuthenticationSchemes = "FrontendJwt")]
    public class SellerCouponsApiController : ControllerBase
    {
        private readonly ICouponService _couponService;
        private readonly ISpanShopDBContext _db;

        public SellerCouponsApiController(ICouponService couponService, ISpanShopDBContext db)
        {
            _couponService = couponService;
            _db = db;
        }

        public class CouponSaveDto
        {
            public string Title { get; set; }
            public string CouponCode { get; set; }
            public int CouponType { get; set; }
            public decimal DiscountValue { get; set; }
            public decimal MinimumSpend { get; set; }
            public decimal? MaximumDiscount { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public int TotalQuantity { get; set; }
            public int PerUserLimit { get; set; }
            public int Status { get; set; }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("無法識別使用者身份");
            }
            return userId;
        }

        private async Task<int> GetStoreIdAsync(int userId)
        {
            var store = await _db.Stores.FirstOrDefaultAsync(s => s.UserId == userId);
            return store?.Id ?? 0;
        }

        [HttpGet]
        public async Task<IActionResult> GetCoupons()
        {
            try
            {
                var userId = GetCurrentUserId();
                var storeId = await GetStoreIdAsync(userId);
                
                if (storeId == 0) return Ok(new { success = true, data = new List<object>() });

                var coupons = await _couponService.GetAllCouponsAsync(storeId);
                
                return Ok(new {
                    success = true,
                    data = coupons.Select(c => new {
                        c.Id,
                        c.Title,
                        c.CouponCode,
                        c.CouponType,
                        c.DiscountValue,
                        c.MinimumSpend,
                        c.MaximumDiscount,
                        c.StartTime,
                        c.EndTime,
                        c.TotalQuantity,
                        c.UsedQuantity,
                        c.Status,
                        c.PerUserLimit
                    })
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCoupon([FromBody] CouponSaveDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var storeId = await GetStoreIdAsync(userId);
                
                if (storeId == 0) return BadRequest(new { success = false, message = "您尚未開通賣場" });

                var coupon = new Coupon
                {
                    SellerId = userId,
                    StoreId = storeId,
                    Title = dto.Title,
                    CouponCode = dto.CouponCode,
                    CouponType = (byte)dto.CouponType,
                    DiscountValue = dto.DiscountValue,
                    MinimumSpend = dto.MinimumSpend,
                    MaximumDiscount = dto.MaximumDiscount,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    TotalQuantity = dto.TotalQuantity,
                    PerUserLimit = dto.PerUserLimit,
                    Status = (byte)dto.Status,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false,
                    UpdatedBy = userId,
                    UsedQuantity = 0,
                    DistributionType = 1, // 1: 領取型
                    ApplyToAll = false,   // 設定為 false，僅限於該賣場使用
                    IsExclusive = false
                };

                await _couponService.CreateCouponAsync(coupon);
                return Ok(new { success = true, message = "優惠券建立成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCoupon(int id, [FromBody] CouponSaveDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var existing = await _couponService.GetCouponByIdAsync(id);
                
                if (existing == null) return NotFound();
                if (existing.SellerId != userId) return Forbid();

                existing.Title = dto.Title;
                existing.CouponCode = dto.CouponCode;
                existing.CouponType = (byte)dto.CouponType;
                existing.DiscountValue = dto.DiscountValue;
                existing.MinimumSpend = dto.MinimumSpend;
                existing.MaximumDiscount = dto.MaximumDiscount;
                existing.StartTime = dto.StartTime;
                existing.EndTime = dto.EndTime;
                existing.TotalQuantity = dto.TotalQuantity;
                existing.PerUserLimit = dto.PerUserLimit;
                existing.Status = (byte)dto.Status;
                existing.UpdatedAt = DateTime.Now;
                existing.UpdatedBy = userId;

                await _couponService.UpdateCouponAsync(existing);
                return Ok(new { success = true, message = "優惠券更新成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var existing = await _couponService.GetCouponByIdAsync(id);
                
                if (existing == null) return NotFound();
                if (existing.SellerId != userId) return Forbid();

                await _couponService.DeleteCouponAsync(id);
                return Ok(new { success = true, message = "優惠券已刪除" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
