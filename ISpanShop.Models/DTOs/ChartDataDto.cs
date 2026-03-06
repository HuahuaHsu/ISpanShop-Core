using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.DTOs
{
	// 通用圖表回傳結構 (給 ApexCharts)
	public class ApexChartDataDto
	{
		public List<string> Labels { get; set; } = new List<string>();
		public List<ChartSeriesDto> Series { get; set; } = new List<ChartSeriesDto>();
	}

	public class ChartSeriesDto
	{
		public string Name { get; set; }
		public List<decimal> Data { get; set; } = new List<decimal>();
	}

	// 專為商品排行使用
	public class TopProductSalesDto
	{
		public string ProductName { get; set; }
		public string CategoryName { get; set; } // 新增：用於統一顏色
		public int SalesVolume { get; set; }
		public decimal SalesRevenue { get; set; }
	}
}
