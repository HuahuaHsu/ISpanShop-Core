#nullable disable
using System;
using System.Collections.Generic;

namespace ISpanShop.Models.EfModels;

public partial class Coupon
{
    public int Id { get; set; }

    public int StoreId { get; set; }

    // 此欄位在 DB 中存在且不可為空
    public int SellerId { get; set; }

    public string CouponCode { get; set; }

    public string Title { get; set; }

    public byte DistributionType { get; set; }

    public byte CouponType { get; set; }

    public decimal DiscountValue { get; set; }

    public decimal MinimumSpend { get; set; }

    public decimal? MaximumDiscount { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int TotalQuantity { get; set; }

    public int UsedQuantity { get; set; }

    public int PerUserLimit { get; set; }

    public bool ApplyToAll { get; set; }

    // 根據報錯，UpdatedBy 在 DB 中存在且不可為空
    public int UpdatedBy { get; set; }
    
    // 暫時保留 UpdatedAt，若報錯則移除
    public DateTime UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; }

    public virtual ICollection<CouponProduct> CouponProducts { get; set; } = new List<CouponProduct>();

    public virtual ICollection<CouponCategory> CouponCategories { get; set; } = new List<CouponCategory>();

    public virtual ICollection<MemberCoupon> MemberCoupons { get; set; } = new List<MemberCoupon>();

    public virtual Store Store { get; set; }
}
