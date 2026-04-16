#nullable disable
using System;
using System.Collections.Generic;

namespace ISpanShop.Models.EfModels;

public partial class MemberCoupon
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CouponId { get; set; }

    public byte UsageStatus { get; set; } // 0:未使用, 1:已使用, 2:已過期, 3:鎖定中

    public long? OrderId { get; set; }

    public DateTime? UsedAt { get; set; }

    public virtual Coupon Coupon { get; set; }

    public virtual Order Order { get; set; }

    public virtual User User { get; set; }
}
