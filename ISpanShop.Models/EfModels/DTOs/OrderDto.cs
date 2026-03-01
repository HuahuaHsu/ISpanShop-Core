using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISpanShop.Common.Enums;

namespace ISpanShop.Models.EfModels.DTOs
{
	public class OrderDto
	{
		public long Id { get; set; }
		public string OrderNumber { get; set; }
		public int UserId { get; set; }
		public string UserName { get; set; } // 關聯使用者名稱
		public int StoreId { get; set; }
		public string StoreName { get; set; } // 關聯商店名稱
		public decimal TotalAmount { get; set; }
		public decimal? ShippingFee { get; set; }
		public decimal FinalAmount { get; set; }
		public OrderStatus Status { get; set; }
		public string RecipientName { get; set; }
		public string RecipientPhone { get; set; }
		public string RecipientAddress { get; set; }
		public DateTime? CreatedAt { get; set; }
	}
}
