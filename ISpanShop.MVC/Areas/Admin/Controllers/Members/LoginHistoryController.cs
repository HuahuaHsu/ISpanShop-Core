using ISpanShop.Models.DTOs.Members;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.MVC.Areas.Admin.Models.Admins;
using ISpanShop.Services.Members;
using Microsoft.AspNetCore.Mvc;
using ISpanShop.MVC.Middleware;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Members
{
	/// <summary>
	/// ?擗???Controller - ???擗??綽潸?
	/// </summary>
	[Area("Admin")]
	[RequireSuperAdmin]
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
				var criteria = new ISpanShop.Models.DTOs.Members.LoginHistoryCriteria
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
