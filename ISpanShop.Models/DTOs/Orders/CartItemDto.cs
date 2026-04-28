using System;
using System.Collections.Generic;

namespace ISpanShop.Models.DTOs.Orders
{
    public class CartItemPromotionDto
    {
        public int PromotionId { get; set; }
        public string Name { get; set; }
        public int PromotionType { get; set; } // 1: 限時特賣, 2: 滿額折扣, 3: 限量搶購
        public string PromotionTypeText => PromotionType switch
        {
            1 => "限時特賣",
            2 => "滿額折扣",
            3 => "限量搶購",
            _ => "促銷活動"
        };
        public decimal Threshold { get; set; }
        public decimal DiscountValue { get; set; }
        public int DiscountType { get; set; } // 1: 固定金額, 2: 百分比
        public string Description { get; set; }
    }

    public class CartItemDto
    {
        public int Id { get; set; } // CartItemID
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public string ProductName { get; set; }
        public string VariantName { get; set; }
        public string ProductImage { get; set; }
        public decimal UnitPrice { get; set; } // 這是原始單價
        public decimal? PromotionPrice { get; set; } // 這是計算後的促銷價 (若無則為 null)
        public int Quantity { get; set; }
        public bool Selected { get; set; } = true; // 前端使用的選中狀態
        public int StoreStatus { get; set; } // 賣場狀態 (1: 正常, 2: 休假中)
        
        // 促銷活動資訊
        public List<CartItemPromotionDto> Promotions { get; set; } = new List<CartItemPromotionDto>();
    }

    public class AddToCartRequestDto
    {
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartItemRequestDto
    {
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int Quantity { get; set; }
    }
}
