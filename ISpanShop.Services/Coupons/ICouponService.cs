using ISpanShop.Models.EfModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISpanShop.Services.Coupons;

public interface ICouponService
{
    Task<IEnumerable<Coupon>> GetAvailableCouponsAsync(int userId, int storeId, decimal subtotal, List<int> productIds);
    Task<(bool IsValid, string Message, Coupon? Coupon)> ValidateCouponAsync(int userId, int couponId, decimal subtotal, List<int> productIds);
    Task<bool> LockCouponAsync(int userId, int couponId, long orderId);
    Task<bool> MarkAsUsedAsync(long orderId);
    Task<bool> ReleaseLockedCouponsAsync(long orderId);
    Task<bool> ReturnCouponAsync(long orderId);

    // 新增：強制解除所有異常鎖定（測試開發用）
    Task<int> ForceReleaseAllExpiredLocksAsync();
    
    // 新增：領取中心與公開領取
    Task<IEnumerable<Coupon>> GetPublicCouponsAsync(int? userId = null);
    Task<(bool Success, string Message)> ClaimCouponAsync(int userId, int couponId);
    
    // 管理後台用
    Task<IEnumerable<Coupon>> GetAllCouponsAsync(int? storeId = null);
    Task<Coupon?> GetCouponByIdAsync(int id);
    Task CreateCouponAsync(Coupon coupon);
    Task UpdateCouponAsync(Coupon coupon);
    Task DeleteCouponAsync(int id);
}
