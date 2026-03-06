using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.DTOs
{
	public class OrderDashboardKpiDto
	{
		// 實收營收 (Net Revenue)
		public decimal NetRevenue { get; set; }
		public decimal NetRevenueGrowthRate { get; set; } // 趨勢對比 (%)

		// 待辦事項 (Action Items)
		public int PendingShipmentCount { get; set; }
		public int PendingRefundCount { get; set; }
		public int LowStockProductCount { get; set; }

		// 連帶率 (Items per Order)
		public decimal ItemsPerOrder { get; set; }
		public decimal ItemsPerOrderGrowthRate { get; set; }

		// 退貨率 (Return Rate)
		public decimal ReturnRate { get; set; }
		public decimal ReturnRateGrowthRate { get; set; } // 注意：此項上升為紅色
	}
}
