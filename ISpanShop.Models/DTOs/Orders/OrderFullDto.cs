using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.DTOs.Orders
{
	public class OrderFullDto : OrderDto
	{
		// 繼承自 OrderDto，並加入明細列表
		public List<OrderDetailDto> Details { get; set; } = new List<OrderDetailDto>();

		// 額外的明細資訊（如有需要可再擴充）
		public string Note { get; set; }
		public DateTime? PaymentDate { get; set; }
		public DateTime? CompletedAt { get; set; }

		// 退貨相關資訊
		public List<string> ReturnRequestImages { get; set; } = new List<string>();

		// 具體退貨內容 (選填)
		public string ReturnReason { get; set; }
		public string ReturnDescription { get; set; }
		public DateTime? ReturnRequestCreatedAt { get; set; }
		public DateTime? RefundDate { get; set; }
	}
}
