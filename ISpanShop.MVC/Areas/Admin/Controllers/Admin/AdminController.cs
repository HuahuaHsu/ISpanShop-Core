using ISpanShop.Models.DTOs.Admins;
using ISpanShop.MVC.Areas.Admin.Models.Admins;
using ISpanShop.Services.Admins;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Admin
{
	[Area("Admin")]
	[Authorize]
	public class AdminController : Controller
	{
		private readonly IAdminService _adminService;

		public AdminController(IAdminService adminService)
		{
			_adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
		}

		public IActionResult Index(string keyword = "", string status = "all", int? adminLevelId = null, string sortColumn = "UserId", string sortDirection = "desc", string activeTab = "tab1", int page = 1, int pageSize = 10)
		{
			var levelIdStr = User.FindFirst("AdminLevelId")?.Value;
			if (levelIdStr != "1")
				return RedirectToAction("Dashboard", "Orders", new { area = "Admin" });

			try
			{
				var criteria = new AdminCriteria
				{
					Keyword = keyword,
					Status = status,
					AdminLevelId = adminLevelId,
					SortColumn = sortColumn,
					IsAscending = sortDirection == "asc",
					PageNumber = page,
					PageSize = pageSize
				};

				var pagedResult = _adminService.SearchPaged(criteria);

				var viewModel = new AdminIndexVm
				{
					PagedResult = pagedResult,
					AdminLevels = _adminService.GetAllAdminLevels().ToList(),
					AllPermissions = _adminService.GetAllPermissions().ToList(),
					NextAccount = _adminService.GetNextAccount(),
					Message = TempData["Message"]?.ToString(),
					ActiveTab = activeTab,
					Keyword = keyword,
					Status = status,
					SelectedAdminLevelId = adminLevelId,
					SortColumn = sortColumn,
					SortDirection = sortDirection,
					CreateForm = new AdminCreateVm
					{
						AdminLevelOptions = _adminService.GetSelectableAdminLevels().ToList()
					}
				};
				return View(viewModel);
			}
			catch (Exception ex)
			{
				return View(new AdminIndexVm
				{
					Message = $"載入失敗: {ex.Message}"
				});
			}
		}

		[HttpGet]
		public IActionResult EditPartial(int id)
		{
			var admin = _adminService.GetAdminById(id);
			if (admin == null) return NotFound();

			var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
				?? User.FindFirst("userid")?.Value;

			var viewModel = new AdminEditVm
			{
				UserId = admin.UserId,
				Account = admin.Account,
				Email = admin.Email,
				AdminLevelId = admin.AdminLevelId,
				IsBlacklisted = admin.IsBlacklisted,
				IsSuperAdmin = admin.AdminLevelId == 1,
				IsSelf = admin.UserId.ToString() == currentUserIdStr,
				AdminLevelOptions = _adminService.GetSelectableAdminLevels().ToList()
			};

			return PartialView("_EditPartial", viewModel);
		}

		[HttpPost]
		public IActionResult CreateAdmin(AdminCreateVm form)
		{
			if (User.FindFirst("AdminLevelId")?.Value != "1")
				return RedirectToAction("Dashboard", "Orders", new { area = "Admin" });

			if (!ModelState.IsValid)
			{
				TempData["Message"] = "表單驗證失敗，請檢查輸入內容";
				TempData["ActiveTab"] = "tab1";
				return RedirectToAction("Index");
			}

			var result = _adminService.CreateAdmin(new AdminCreateDto
			{
				Password = form.Password,
				AdminLevelId = form.AdminLevelId
			});

			TempData["Message"] = result.Message;
			TempData["ActiveTab"] = "tab1";
			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult UpdateAdmin(AdminUpdateDto dto)
		{
			if (User.FindFirst("AdminLevelId")?.Value != "1")
				return RedirectToAction("Dashboard", "Orders", new { area = "Admin" });

			if (!ModelState.IsValid)
			{
				TempData["Message"] = "資料格式錯誤";
				TempData["ActiveTab"] = "tab1";
				return RedirectToAction("Index");
			}

			var result = _adminService.UpdateAdmin(dto);
			TempData["Message"] = result.Message;
			TempData["ActiveTab"] = "tab1";
			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult DeactivateAdmin(int userId)
		{
			if (User.FindFirst("AdminLevelId")?.Value != "1")
				return RedirectToAction("Dashboard", "Orders", new { area = "Admin" });

			var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
				?? User.FindFirst("userid")?.Value;

			if (!int.TryParse(currentUserIdStr, out int currentUserId))
			{
				TempData["Message"] = "無法識別當前登入的管理員";
				TempData["ActiveTab"] = "tab1";
				return RedirectToAction("Index");
			}

			var result = _adminService.DeactivateAdmin(userId, currentUserId);
			TempData["Message"] = result.Message;
			TempData["ActiveTab"] = "tab1";
			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult ResetPassword(AdminResetPasswordDto dto)
		{
			if (User.FindFirst("AdminLevelId")?.Value != "1")
				return RedirectToAction("Dashboard", "Orders", new { area = "Admin" });

			if (!ModelState.IsValid)
			{
				TempData["Message"] = "資料格式錯誤";
				TempData["ActiveTab"] = "tab1";
				return RedirectToAction("Index");
			}

			var result = _adminService.ResetAdminPassword(dto);
			TempData["Message"] = result.Message;
			TempData["ActiveTab"] = "tab1";
			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult CreateAdminLevel(AdminLevelCreateVm form)
		{
			if (User.FindFirst("AdminLevelId")?.Value != "1")
				return RedirectToAction("Dashboard", "Orders", new { area = "Admin" });

			if (!ModelState.IsValid)
			{
				TempData["Message"] = "表單驗證失敗，請檢查輸入內容";
				TempData["ActiveTab"] = "tab2";
				return RedirectToAction("Index");
			}

			var result = _adminService.CreateAdminLevel(new AdminLevelCreateDto
			{
				LevelName = form.LevelName,
				Description = form.Description,
				PermissionIds = form.PermissionIds
			});

			TempData["Message"] = result.Message;
			TempData["ActiveTab"] = "tab2";
			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult UpdateAdminLevel(AdminLevelUpdateDto dto)
		{
			if (User.FindFirst("AdminLevelId")?.Value != "1")
				return RedirectToAction("Dashboard", "Orders", new { area = "Admin" });

			var result = _adminService.UpdateAdminLevelConfig(dto);
			TempData["Message"] = result.Message;
			TempData["ActiveTab"] = "tab2";
			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult DeleteAdminLevel(int adminLevelId)
		{
			if (User.FindFirst("AdminLevelId")?.Value != "1")
				return RedirectToAction("Dashboard", "Orders", new { area = "Admin" });

			var result = _adminService.DeleteAdminLevel(adminLevelId);
			TempData["Message"] = result.Message;
			TempData["ActiveTab"] = "tab2";
			return RedirectToAction("Index");
		}
	}
}