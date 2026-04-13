using ISpanShop.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.DTOs.Orders
{
	public class OrderSearchDto
	{
		// A1. 基礎資訊 (整合了 訂單號、用戶ID、收件人姓名/電話)
		public string Keyword { get; set; } 

		// 指定會員 ID 查詢 (前台需求)
		public int? UserId { get; set; }

		// A2. 訂單狀態 (支援多選)
		public List<int> Statuses { get; set; } = new List<int>();

		// A3. 金額區間
		public decimal? MinAmount { get; set; }
		public decimal? MaxAmount { get; set; }

		// A4 & A5. 日期篩選
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public int DateDimension { get; set; } = 1; // 1=下單, 2=付款, 3=完成, 4=退貨申請, 5=退款完成

		// A6. 商店屬性
		public int? StoreId { get; set; }
		public string StoreName { get; set; }

		// C. 庫存狀態篩選 (1=充足, 2=不足)
		public int? StockStatus { get; set; }

		// B. 動態排序與分頁
		public string SortBy { get; set; } 
		public bool IsDescending { get; set; } = true;
		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}
}
