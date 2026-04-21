using System;
using System.Collections.Generic;

namespace ISpanShop.Models.DTOs.Orders
{
    public class FrontReturnRequestDto
    {
        public string ReasonCategory { get; set; }
        public string ReasonDescription { get; set; }
        public List<ReturnItemDto> Items { get; set; }
        public List<string> ImageUrls { get; set; }
    }

    public class ReturnItemDto
    {
        public long OrderDetailId { get; set; }
        public int Quantity { get; set; }
    }
}
