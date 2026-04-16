using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISpanShop.Services.Coupons;

public class CouponService : ICouponService
{
    private readonly ISpanShopDBContext _context;

    public CouponService(ISpanShopDBContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Coupon>> GetAvailableCouponsAsync(int userId, int storeId, decimal subtotal, List<int> productIds)
    {
        var now = DateTime.Now;
        
        // 1. 取得會員持有的、未使用的券
        var memberCoupons = await _context.MemberCoupons
            .Include(mc => mc.Coupon)
            .ThenInclude(c => c.CouponProducts)
            .Include(mc => mc.Coupon)
            .ThenInclude(c => c.CouponCategories)
            .Where(mc => mc.UserId == userId && 
                         mc.UsageStatus == 0 && 
                         mc.Coupon.StartTime <= now && 
                         mc.Coupon.EndTime >= now)
            .ToListAsync();

        var available = new List<Coupon>();

        foreach (var mc in memberCoupons)
        {
            var coupon = mc.Coupon;
            
            // 2. 檢查商店限制
            // 如果不是全站通用 (ApplyToAll)，且商店 ID 不匹配，則跳過
            if (!coupon.ApplyToAll && coupon.StoreId != storeId) continue;

            // 3. 檢查低消門檻
            if (subtotal < coupon.MinimumSpend) continue;

            // 4. 如果全站通用（ApplyToAll），直接加入；如果不是，則不額外檢查產品（簡化邏輯以確保券能出現）
            // 注意：這裡我們暫時放寬產品限定，確保使用者能看到券，如果產品不符，驗證時會再擋下
            available.Add(coupon);
        }

        return available;
    }

    public async Task<(bool IsValid, string Message, Coupon? Coupon)> ValidateCouponAsync(int userId, int couponId, decimal subtotal, List<int> productIds)
    {
        var now = DateTime.Now;
        var memberCoupon = await _context.MemberCoupons
            .Include(mc => mc.Coupon)
            .ThenInclude(c => c.CouponProducts)
            .FirstOrDefaultAsync(mc => mc.UserId == userId && mc.CouponId == couponId);

        if (memberCoupon == null) return (false, "您沒有這張優惠券", null);
        if (memberCoupon.UsageStatus != 0) return (false, "優惠券已使用或無效", null);
        
        var coupon = memberCoupon.Coupon;
        if (coupon.StartTime > now) return (false, "優惠券尚未開始使用", null);
        if (coupon.EndTime < now) return (false, "優惠券已過期", null);
        if (subtotal < coupon.MinimumSpend) return (false, $"未達低消門檻 {coupon.MinimumSpend:C0}", null);

        // 檢查範圍 (Scope Check)
        if (!coupon.ApplyToAll)
        {
            var eligibleProducts = coupon.CouponProducts.Select(cp => cp.ProductId).ToList();
            bool hasEligibleProduct = productIds.Any(pid => eligibleProducts.Contains(pid));
            
            if (!hasEligibleProduct) return (false, "此優惠券不適用於您的訂單商品", null);
        }

        return (true, "驗證成功", coupon);
    }

    public async Task<bool> LockCouponAsync(int userId, int couponId, long orderId)
    {
        var memberCoupon = await _context.MemberCoupons
            .FirstOrDefaultAsync(mc => mc.UserId == userId && mc.CouponId == couponId && mc.UsageStatus == 0);

        if (memberCoupon == null) return false;

        memberCoupon.UsageStatus = 3; // 鎖定中
        memberCoupon.OrderId = orderId;

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }

    public async Task<bool> MarkAsUsedAsync(long orderId)
    {
        var memberCoupon = await _context.MemberCoupons
            .FirstOrDefaultAsync(mc => mc.OrderId == orderId && mc.UsageStatus == 3);

        if (memberCoupon == null) return false;

        memberCoupon.UsageStatus = 1; // 已使用
        memberCoupon.UsedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReleaseLockedCouponsAsync(long orderId)
    {
        var memberCoupon = await _context.MemberCoupons
            .FirstOrDefaultAsync(mc => mc.OrderId == orderId && mc.UsageStatus == 3);

        if (memberCoupon == null) return false;

        memberCoupon.UsageStatus = 0; // 還原為未使用
        memberCoupon.OrderId = null;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> ForceReleaseAllExpiredLocksAsync()
    {
        // 暴力解除：只要是狀態為 3 (鎖定中) 的，通通改回 0 (未使用)
        var lockedCoupons = await _context.MemberCoupons
            .Where(mc => mc.UsageStatus == 3)
            .ToListAsync();

        int count = lockedCoupons.Count;
        foreach (var mc in lockedCoupons)
        {
            mc.UsageStatus = 0;
            mc.OrderId = null;
        }

        if (count > 0) await _context.SaveChangesAsync();
        return count;
    }

    public async Task<IEnumerable<Coupon>> GetPublicCouponsAsync(int? userId = null)
    {
        var now = DateTime.Now;
        // 取得所有公開領取、且在期限內、且還有餘額的優惠券
        // 移除 Include(c => c.MemberCoupons) 以提升效能
        var query = _context.Coupons
            .Where(c => c.DistributionType == 1 && 
                        c.StartTime <= now && 
                        c.EndTime >= now &&
                        c.UsedQuantity < c.TotalQuantity);

        var coupons = await query.ToListAsync();
        return coupons;
    }

    public async Task<(bool Success, string Message)> ClaimCouponAsync(int userId, int couponId)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        
        return await strategy.ExecuteAsync(async () =>
        {
            var now = DateTime.Now;
            var coupon = await _context.Coupons
                .Include(c => c.MemberCoupons)
                .FirstOrDefaultAsync(c => c.Id == couponId);

            if (coupon == null) return (false, "找不到此優惠券");
            if (coupon.DistributionType != 1) return (false, "此優惠券不開放公開領取");
            if (coupon.StartTime > now) return (false, "領取尚未開始");
            if (coupon.EndTime < now) return (false, "領取已結束");
            if (coupon.UsedQuantity >= coupon.TotalQuantity) return (false, "優惠券已領取完畢");

            int claimedCount = await _context.MemberCoupons
                .CountAsync(mc => mc.UserId == userId && mc.CouponId == couponId);
            
            if (claimedCount >= coupon.PerUserLimit) return (false, $"您已達領取上限 ({coupon.PerUserLimit}次)");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var memberCoupon = new MemberCoupon
                {
                    UserId = userId,
                    CouponId = couponId,
                    UsageStatus = 0
                };

                _context.MemberCoupons.Add(memberCoupon);
                coupon.UsedQuantity += 1;
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return (true, "領取成功");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "領取失敗，資料庫發生錯誤");
            }
        });
    }

    public async Task<IEnumerable<Coupon>> GetAllCouponsAsync(int? storeId = null)
    {
        var query = _context.Coupons.AsQueryable();
        if (storeId.HasValue) query = query.Where(c => c.StoreId == storeId.Value);
        return await query.OrderByDescending(c => c.StartTime).ToListAsync();
    }

    public async Task<Coupon?> GetCouponByIdAsync(int id)
    {
        return await _context.Coupons
            .Include(c => c.CouponProducts)
            .Include(c => c.CouponCategories)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task CreateCouponAsync(Coupon coupon)
    {
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCouponAsync(Coupon coupon)
    {
        _context.Entry(coupon).State = EntityState.Modified;
        // 防止 RowVersion 被修改
        _context.Entry(coupon).Property(x => x.RowVersion).IsModified = false;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCouponAsync(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon != null)
        {
            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();
        }
    }
}
