using System;
using System.Collections.Generic;

namespace ISpanShop.Models.EfModels;

public partial class ReturnRequestItem
{
    public long Id { get; set; }

    public long ReturnRequestId { get; set; }

    public long OrderDetailId { get; set; }

    public int Quantity { get; set; }

    public virtual OrderDetail OrderDetail { get; set; }

    public virtual ReturnRequest ReturnRequest { get; set; }
}
