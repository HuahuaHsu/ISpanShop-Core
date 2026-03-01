using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.EfModels.DTOs
{
	public class PagedResultDto<T>
	{
		public List<T> Items { get; set; } = new();
		public int TotalCount { get; set; }     // 總筆數
		public int PageNumber { get; set; }      // 當前頁碼
		public int PageSize { get; set; }        // 每頁幾筆
		public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize); // 總頁數
	}
}
