using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.DTOs
{
	public class CheckoutRequestDTO
	{
		public int UserId { get; set; }
		public int StoreId { get; set; }
		public bool UsePoints { get; set; } // 使用者是否勾選折抵
		public List<CartItemDTO> Items { get; set; } // 購物車內容
		public string RecipientName { get; set; }
		public string RecipientPhone { get; set; }
		public string RecipientAddress { get; set; }
	}
}
