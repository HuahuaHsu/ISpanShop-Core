using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.DTOs.Orders;

public class CheckoutRequestDto
{
	public int UserId { get; set; }
	public int StoreId { get; set; }
	public bool UsePoints { get; set; } // 使用者是否勾選折抵
	public int? CouponId { get; set; } // 新增優惠券 ID
	public decimal? LevelDiscount { get; set; } // 新增會員等級折扣金額
	
	public List<CheckoutItemDto> Items { get; set; } // 購物車內容
	public string RecipientName { get; set; }
	public string RecipientPhone { get; set; }
	public string RecipientAddress { get; set; }
	public string PaymentMethod { get; set; } = "ECPay";
}

public class CheckoutItemDto
{
	public int ProductId { get; set; }
	public int? VariantId { get; set; }
	public string ProductName { get; set; }
	public string VariantName { get; set; }
	public decimal UnitPrice { get; set; }
	public int Quantity { get; set; }
}
