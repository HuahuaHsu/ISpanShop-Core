#nullable disable
using System;
using System.Collections.Generic;

namespace ISpanShop.Models.EfModels;

public partial class CouponCategory
{
    public int CouponId { get; set; }

    public int CategoryId { get; set; }

    public virtual Coupon Coupon { get; set; }

    public virtual Category Category { get; set; }
}
