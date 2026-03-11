using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.DTOs
{
	public class CartItemDTO
	{
		public int ProductId { get; set; }
		public int VariantId { get; set; }
		public string ProductName { get; set; }
		public string VariantName { get; set; }
		public decimal UnitPrice { get; set; }
		public int Quantity { get; set; }
	}
}
