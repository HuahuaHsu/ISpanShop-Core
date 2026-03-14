using ISpanShop.Models.DTOs.Members;
using ISpanShop.Models.DTOs.Common;
using System.Collections.Generic;
using System.Linq;

namespace ISpanShop.MVC.Areas.Admin.Models.Admins
{
	/// <summary>
	/// 登入紀錄列表 ViewModel - 用於呈現登入紀錄的後台管理界面
	/// </summary>
	public class LoginHistoryIndexVm
	{
		/// <summary>
		/// 登入紀錄列表
		/// </summary>
		public List<LoginHistoryItemVm> LoginHistories { get; set; } = new List<LoginHistoryItemVm>();

		/// <summary>
		/// 分頁資訊
		/// </summary>
		public PaginationInfoVm PaginationInfo { get; set; } = new PaginationInfoVm();

		/// <summary>
		/// 目前查詢條件
		/// </summary>
		public LoginHistoryCriteriaVm SearchCriteria { get; set; } = new LoginHistoryCriteriaVm();

		/// <summary>
		/// 操作結果訊息（成功/失敗）
		/// </summary>
		public string Message { get; set; }
	}

	/// <summary>
	/// 分頁資訊 ViewModel
	/// </summary>
	public class PaginationInfoVm
	{
		/// <summary>
		/// 資料總筆數
		/// </summary>
		public int TotalCount { get; set; }

		/// <summary>
		/// 總頁數
		/// </summary>
		public int TotalPages { get; set; }

		/// <summary>
		/// 目前頁碼
		/// </summary>
		public int CurrentPageNumber { get; set; } = 1;

		/// <summary>
		/// 每頁筆數
		/// </summary>
		public int PageSize { get; set; } = 10;

		/// <summary>
		/// 是否有下一頁
		/// </summary>
		public bool HasNextPage { get; set; }

		/// <summary>
		/// 是否有上一頁
		/// </summary>
		public bool HasPreviousPage { get; set; }

		/// <summary>
		/// 頁碼清單（用於產生分頁按鈕）
		/// </summary>
		public List<int> PageNumbers { get; set; } = new List<int>();
	}

	/// <summary>
	/// 查詢條件 ViewModel - 用於回填表單
	/// </summary>
	public class LoginHistoryCriteriaVm
	{
		/// <summary>
		/// 搜尋關鍵字
		/// </summary>
		public string Keyword { get; set; }

		/// <summary>
		/// 篩選登入狀態
		/// </summary>
		public string IsSuccessfulFilter { get; set; }  // "all", "success", "failure"

		/// <summary>
		/// 排序欄位
		/// </summary>
		public string SortColumn { get; set; } = "LoginTime";

		/// <summary>
		/// 排序方向
		/// </summary>
		public string SortDirection { get; set; } = "desc";  // "asc" 或 "desc"

		/// <summary>
		/// 每頁筆數
		/// </summary>
		public int PageSize { get; set; } = 10;
	}

	/// <summary>
	/// 登入紀錄項目 ViewModel - 用於在列表中顯示單筆登入紀錄
	/// </summary>
	public class LoginHistoryItemVm
	{
		/// <summary>
		/// 紀錄ID
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// 使用者帳號 (嘗試登入帳號)
		/// </summary>
		public string UserAccount { get; set; }

		/// <summary>
		/// 登入時間（格式化為字串）
		/// </summary>
		public string LoginTimeFormatted { get; set; }

		/// <summary>
		/// IP位址
		/// </summary>
		public string IpAddress { get; set; }

		/// <summary>
		/// 登入狀態文字（成功/失敗/未知）
		/// </summary>
		public string StatusText { get; set; }

		/// <summary>
		/// 登入狀態 CSS 類別（用於 Badge 樣式）
		/// </summary>
		public string StatusClass { get; set; }

		/// <summary>
		/// 原始的成功/失敗布林值
		/// </summary>
		public bool IsSuccess { get; set; }
	}

	/// <summary>
	/// DTO 轉 ViewModel 的轉換擴充方法
	/// </summary>
	public static class LoginHistoryMappingExtensions
	{
		/// <summary>
		/// 將 LoginHistoryDto 轉換為 LoginHistoryItemVm
		/// </summary>
		public static LoginHistoryItemVm ToViewModel(this LoginHistoryDto dto)
		{
			return new LoginHistoryItemVm
			{
				Id = dto.Id,
				UserAccount = dto.AttemptedAccount,
				LoginTimeFormatted = dto.LoginTime.ToString("yyyy-MM-dd HH:mm:ss"),
				IpAddress = string.IsNullOrEmpty(dto.Ipaddress) ? "-" : dto.Ipaddress,
				IsSuccess = dto.IsSuccess,
				StatusText = GetStatusText(dto.IsSuccess),
				StatusClass = GetStatusClass(dto.IsSuccess)
			};
		}

		/// <summary>
		/// 將 PagedResult 轉換為含分頁資訊的 LoginHistoryIndexVm
		/// </summary>
		public static LoginHistoryIndexVm ToPagedViewModel(
			this PagedResult<LoginHistoryDto> pagedResult,
			LoginHistoryCriteria criteria)
		{
			// 轉換紀錄項目
			var items = pagedResult.Data
				.Select(lh => lh.ToViewModel())
				.ToList();

			// 生成頁碼清單 (顯示最多 5 個頁碼按鈕)
			var pageNumbers = GeneratePageNumbers(pagedResult.CurrentPage, pagedResult.TotalPages);

			var vm = new LoginHistoryIndexVm
			{
				LoginHistories = items,
				PaginationInfo = new PaginationInfoVm
				{
					TotalCount = pagedResult.TotalCount,
					TotalPages = pagedResult.TotalPages,
					CurrentPageNumber = pagedResult.CurrentPage,
					PageSize = pagedResult.PageSize,
					HasNextPage = pagedResult.CurrentPage < pagedResult.TotalPages,
					HasPreviousPage = pagedResult.CurrentPage > 1,
					PageNumbers = pageNumbers
				},
				SearchCriteria = new LoginHistoryCriteriaVm
				{
					Keyword = criteria.Keyword,
					IsSuccessfulFilter = criteria.IsSuccessful switch
					{
						true => "success",
						false => "failure",
						_ => "all"
					},
					SortColumn = criteria.SortColumn,
					SortDirection = criteria.IsAscending ? "asc" : "desc",
					PageSize = criteria.PageSize
				}
			};

			return vm;
		}

		/// <summary>
		/// 生成頁碼清單 (顯示最多 5 個頁碼)
		/// </summary>
		private static List<int> GeneratePageNumbers(int currentPage, int totalPages)
		{
			var pageNumbers = new List<int>();

			if (totalPages <= 5)
			{
				// 如果總頁數 <= 5，顯示全部
				for (int i = 1; i <= totalPages; i++)
				{
					pageNumbers.Add(i);
				}
			}
			else
			{
				// 否則顯示當前頁碼前後各 2 頁
				int start = currentPage - 2;
				if (start < 1) start = 1;

				int end = start + 4;
				if (end > totalPages) end = totalPages;

				if (end - start < 4)
				{
					start = end - 4;
					if (start < 1) start = 1;
				}

				for (int i = start; i <= end; i++)
				{
					pageNumbers.Add(i);
				}
			}

			return pageNumbers;
		}

		/// <summary>
		/// 取得登入狀態文字
		/// </summary>
		private static string GetStatusText(bool isSuccess)
		{
			return isSuccess ? "登入成功" : "登入失敗";
		}

		/// <summary>
		/// 取得登入狀態的 CSS 類別
		/// </summary>
		private static string GetStatusClass(bool isSuccess)
		{
			return isSuccess ? "bg-green-100 text-green-800" : "bg-red-100 text-red-800";
		}
	}
}
