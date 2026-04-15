using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.DTOs.Orders
{
	public class OrderListDto
	{
		public int OrderId { get; set; }
		public string OrderUuid { get; set; }
		public string MemberName { get; set; }
		public decimal TotalAmount { get; set; }
		public int StatusId { get; set; }
		public string StatusName { get; set; }
		public DateTime OrderDate { get; set; }
		public DateTime? PaymentDate { get; set; }
		public DateTime? CompletedAt { get; set; }
		public string RecipientName { get; set; }
		public string RecipientPhone { get; set; }
		public string StoreName { get; set; }

		// 出貨工作台專用：揀貨摘要 (例如: A商品x2, B商品x1)
		public string ItemsSummary { get; set; }
		// 出貨等待時長 (毫秒或 TimeSpan 字串)
		public string WaitingTime { get; set; }

		// 退貨相關
		public DateTime? ReturnRequestCreatedAt { get; set; }
		public DateTime? RefundDate { get; set; }
	}
}
