using System;

namespace ISpanShop.Models.DTOs.Orders
{
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
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public bool Selected { get; set; } = true; // 前端使用的選中狀態
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
