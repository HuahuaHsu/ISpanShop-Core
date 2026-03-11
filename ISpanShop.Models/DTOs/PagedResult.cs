using System.Collections.Generic;

namespace ISpanShop.Models.DTOs
{
	/// <summary>
	/// 分頁查詢結果包裝器 - 包含資料及分頁資訊
	/// </summary>
	/// <typeparam name="T">資料類型</typeparam>
	public class PagedResult<T> where T : class
	{
		/// <summary>
		/// 當前頁的資料
		/// </summary>
		public List<T> Items { get; set; } = new List<T>();

		/// <summary>
		/// 資料總筆數
		/// </summary>
		public int TotalCount { get; set; }

		/// <summary>
		/// 每頁筆數
		/// </summary>
		public int PageSize { get; set; }

		/// <summary>
		/// 目前頁碼
		/// </summary>
		public int PageNumber { get; set; }

		/// <summary>
		/// 總頁數
		/// </summary>
		public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

		/// <summary>
		/// 是否有下一頁
		/// </summary>
		public bool HasNextPage => PageNumber < TotalPages;

		/// <summary>
		/// 是否有上一頁
		/// </summary>
		public bool HasPreviousPage => PageNumber > 1;

		/// <summary>
		/// 是否為第一頁
		/// </summary>
		public bool IsFirstPage => PageNumber == 1;

		/// <summary>
		/// 是否為最後一頁
		/// </summary>
		public bool IsLastPage => PageNumber >= TotalPages;

		/// <summary>
		/// 建構子
		/// </summary>
		public PagedResult()
		{
		}

		/// <summary>
		/// 建構子
		/// </summary>
		public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
		{
			Items = items;
			TotalCount = totalCount;
			PageNumber = pageNumber;
			PageSize = pageSize;
		}
	}
}
