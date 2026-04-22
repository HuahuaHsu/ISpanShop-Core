using System;

namespace ISpanShop.Models.DTOs.Promotions
{
    public class SellerPromotionListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int PromotionType { get; set; }
        public string PromotionTypeLabel { get; set; } = string.Empty;
        public decimal? DiscountValue { get; set; }
        public decimal? MinimumAmount { get; set; }
        public int ProductCount { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string? RejectReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
