using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.DTOs
{
	public class DashboardKpiRawDataDto
	{
		public decimal NetRevenue { get; set; }
		public decimal PrevNetRevenue { get; set; }
		public int TotalOrders { get; set; }
		public int PrevTotalOrders { get; set; }
		public int ReturnOrders { get; set; }
		public int PrevReturnOrders { get; set; }
		public int TotalItemsSold { get; set; }
		public int PrevTotalItemsSold { get; set; }
		public int PendingShipmentCount { get; set; }
		public int PendingRefundCount { get; set; }
		public int LowStockProductCount { get; set; }

		// 新增：顧客與行為
		public int NewMemberCount { get; set; }
		public int PrevNewMemberCount { get; set; }
		public int UniqueMemberCount { get; set; } // 本期下單過的不重複會員數
		public int PrevUniqueMemberCount { get; set; }
		public int RepeatMemberCount { get; set; } // 本期有兩次以上下單紀錄的會員
		public int PrevRepeatMemberCount { get; set; }

		// 新增：營運效率
		public double TotalFulfillmentTicks { get; set; } // 總出貨時長 (Ticks)
		public double PrevTotalFulfillmentTicks { get; set; }
		public int ShippedOrderCount { get; set; } // 本期內有標記出貨的訂單數
		public int PrevShippedOrderCount { get; set; }
	}
}
