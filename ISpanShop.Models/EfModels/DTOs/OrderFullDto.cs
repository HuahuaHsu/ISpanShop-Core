using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.EfModels.DTOs
{
	public class OrderFullDto : OrderDto
	{
		// 繼承自 OrderDto，並加入明細列表
		public List<OrderDetailDto> Details { get; set; } = new List<OrderDetailDto>();

		// 額外的明細資訊（如有需要可再擴充）
		public string Note { get; set; }
		public DateTime? PaymentDate { get; set; }
		public DateTime? CompletedAt { get; set; }
	}
}
