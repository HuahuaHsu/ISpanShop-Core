using ISpanShop.Models.DTOs.Admins;
using ISpanShop.MVC.Areas.Admin.Models.Admins;
using ISpanShop.Services.Admins;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Identities
{
	[Area("Admin")]
	[Authorize(Roles = "SuperAdmin")]
	public class AdminController : Controller
	{
		private readonly IAdminService _adminService;

		public AdminController(IAdminService adminService)
		{
			_adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
		}

		public IActionResult Index()
		{
			try
			{
				var viewModel = new AdminIndexVm
				{
					Admins = _adminService.GetAllAdmins().ToList(),
					AdminLevels = _adminService.GetAllAdminLevels().ToList(),
					AllPermissions = _adminService.GetAllPermissions().ToList(),
					NextAccount = _adminService.GetNextAccount(),
					Message = TempData["Message"]?.ToString(),
					ActiveTab = TempData["ActiveTab"]?.ToString() ?? "tab1",
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

		[HttpPost]
		public IActionResult CreateAdmin(AdminCreateVm form)
		{
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
			var result = _adminService.UpdateAdmin(dto);
			TempData["Message"] = result.Message;
			TempData["ActiveTab"] = "tab1";
			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult DeactivateAdmin(int userId)
		{
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
		public IActionResult CreateAdminLevel(AdminLevelCreateVm form)
		{
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
			var result = _adminService.UpdateAdminLevelConfig(dto);
			TempData["Message"] = result.Message;
			TempData["ActiveTab"] = "tab2";
			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult DeleteAdminLevel(int adminLevelId)
		{
			var result = _adminService.DeleteAdminLevel(adminLevelId);
			TempData["Message"] = result.Message;
			TempData["ActiveTab"] = "tab2";
			return RedirectToAction("Index");
		}
	}
}