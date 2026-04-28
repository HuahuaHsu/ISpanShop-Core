using System;

namespace ISpanShop.Models.DTOs.Stores
{
    public class SellerReplyDto
    {
        public long OrderId { get; set; }
        public string ReplyText { get; set; }
    }
}
