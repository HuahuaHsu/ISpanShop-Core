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
	}
}
