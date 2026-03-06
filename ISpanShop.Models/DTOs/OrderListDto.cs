using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.DTOs
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
	}
}
