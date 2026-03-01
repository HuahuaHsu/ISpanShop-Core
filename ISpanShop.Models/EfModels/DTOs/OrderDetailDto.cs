using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.EfModels.DTOs
{
	public class OrderDetailDto
	{
		public long Id { get; set; }
		public long OrderId { get; set; }
		public int ProductId { get; set; }
		public int? VariantId { get; set; }
		public string ProductName { get; set; }
		public string VariantName { get; set; }
		public string SkuCode { get; set; }
		public string CoverImage { get; set; }
		public decimal Price { get; set; }
		public int Quantity { get; set; }
		public decimal SubTotal => Price * Quantity; // 小計
	}
}
