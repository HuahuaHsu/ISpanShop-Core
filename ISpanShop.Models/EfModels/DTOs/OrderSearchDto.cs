using ISpanShop.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.EfModels.DTOs
{
	public class OrderSearchDto
	{
		public string OrderNumber { get; set; }
		public OrderStatus? Status { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }

		// 分頁參數
		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}
}
