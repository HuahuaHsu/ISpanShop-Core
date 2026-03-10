using ISpanShop.Models.DTOs;
using ISpanShop.MVC.Models.LoginHistories;
using ISpanShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Controllers
{
	/// <summary>
	/// 登入紀錄管理 Controller - 處理登入紀錄的後台管理
	/// </summary>
	public class LoginHistoryController : Controller
	{
		private readonly ILoginHistoryService _loginHistoryService;

		public LoginHistoryController(ILoginHistoryService loginHistoryService)
		{
			_loginHistoryService = loginHistoryService ?? throw new ArgumentNullException(nameof(loginHistoryService));
		}

		/// <summary>
		/// 登入紀錄列表頁 - 顯示所有登入紀錄（支援搜尋、排序、分頁）
		/// </summary>
		public IActionResult Index(
			string keyword = "",
			string isSuccessful = "all",
			string sortColumn = "LoginTime",
			string sortDirection = "desc",
			int pageNumber = 1,
			int pageSize = 10)
		{
			try
			{
				// 建立查詢條件
				var criteria = new LoginHistoryCriteria
				{
					Keyword = keyword,
					IsSuccessful = isSuccessful switch
					{
						"success" => true,
						"failure" => false,
						_ => null
					},
					SortColumn = sortColumn,
					IsAscending = sortDirection == "asc",
					PageNumber = pageNumber,
					PageSize = pageSize
				};

				// 執行查詢
				var pagedResult = _loginHistoryService.SearchPagedLoginHistories(criteria);

				// 轉換為 ViewModel
				var viewModel = pagedResult.ToPagedViewModel(criteria);
				viewModel.Message = TempData["Message"]?.ToString();

				return View(viewModel);
			}
			catch (Exception ex)
			{
				var emptyVm = new LoginHistoryIndexVm
				{
					LoginHistories = new List<LoginHistoryItemVm>(),
					Message = $"載入登入紀錄失敗: {ex.Message}"
				};
				return View(emptyVm);
			}
		}
	}
}
