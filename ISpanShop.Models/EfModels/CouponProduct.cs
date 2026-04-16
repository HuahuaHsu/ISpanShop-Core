#nullable disable
using System;
using System.Collections.Generic;

namespace ISpanShop.Models.EfModels;

public partial class CouponProduct
{
    public int CouponId { get; set; }

    public int ProductId { get; set; }

    public virtual Coupon Coupon { get; set; }

    public virtual Product Product { get; set; }
}
