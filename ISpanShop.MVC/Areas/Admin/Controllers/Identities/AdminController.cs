using ISpanShop.Models.DTOs.Admins;
using ISpanShop.MVC.Areas.Admin.Models.Admins;
using ISpanShop.Services.Admins;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Identities
{
	[Area("Admin")]
	//[Authorize(Roles = "SuperAdmin")] //測試功能階不需要權限
	public class AdminController : Controller
	{
		private readonly IAdminService _adminService;

		public AdminController(IAdminService adminService)
		{
			_adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
		}

		/// <summary>
		/// 管理員列表頁 - 顯示所有管理員及新增/停用選項
		/// </summary>
		public IActionResult Index()
		{
			try
			{
				var admins = _adminService.GetAllAdmins().ToList();
				var permissions = _adminService.GetAllPermissions().ToList();
				var adminLevels = _adminService.GetSelectableAdminLevels().ToList();

				var viewModel = new AdminIndexVm
				{
					Admins = admins,
					PermissionOptions = permissions,
					Message = TempData["Message"]?.ToString(),
					GeneratedAccount = TempData["GeneratedAccount"]?.ToString(),
					CreateForm = new AdminCreateVm
					{
						AdminLevelOptions = adminLevels
					}
				};
				return View(viewModel);
			}
			catch (Exception ex)
			{
				var emptyVm = new AdminIndexVm
				{
					Admins = new List<AdminDto>(),
					PermissionOptions = new List<AdminPermissionDto>(),
					CreateForm = new AdminCreateVm(),
					Message = $"載入管理員列表失敗: {ex.Message}"
				};
				return View(emptyVm);
			}
		}

		/// <summary>
		/// 新增管理員
		/// </summary>
		[HttpPost]
		public IActionResult CreateAdmin(AdminCreateVm form)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					TempData["Message"] = "表單驗證失敗，請檢查輸入內容";
					return RedirectToAction("Index");
				}

				// 轉換為 DTO
				var dto = new AdminCreateDto
				{
					Password = form.Password,
					AdminLevelId = form.AdminLevelId
				};

				// 呼叫服務層
				var result = _adminService.CreateAdmin(dto);

				if (result.IsSuccess)
				{
					TempData["Message"] = result.Message;
					TempData["GeneratedAccount"] = result.GeneratedAccount;
				}
				else
				{
					TempData["Message"] = result.Message;
				}
			}
			catch (Exception ex)
			{
				TempData["Message"] = $"新增管理員失敗: {ex.Message}";
			}

			return RedirectToAction("Index");
		}

		/// <summary>
		/// 停用管理員
		/// </summary>
		[HttpPost]
		public IActionResult DeactivateAdmin(int userId)
		{
			try
			{
				// 從 Claims 取得 currentUserId
				var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
					?? User.FindFirst("userid")?.Value;

				if (!int.TryParse(currentUserIdStr, out int currentUserId))
				{
					TempData["Message"] = "無法識別當前登入的管理員";
					return RedirectToAction("Index");
				}

				// 呼叫服務層
				var result = _adminService.DeactivateAdmin(userId, currentUserId);

				TempData["Message"] = result.Message;
			}
			catch (Exception ex)
			{
				TempData["Message"] = $"停用管理員失敗: {ex.Message}";
			}

			return RedirectToAction("Index");
		}

		/// <summary>
		/// 設置 Super Admin Cookie (測試用途)
		/// 直接將 Cookie 發給瀏覽器，讓系統「以為」已經是 Super Admin
		/// </summary>
		[HttpGet]
		public IActionResult SetSuperAdminCookie(int adminId = 1)
		{
			try
			{
				// 驗證此 admin ID 是否真的是 Super Admin
				var admin = _adminService.GetAllAdmins()
					.FirstOrDefault(a => a.UserId == adminId && a.RoleName == "SuperAdmin");

				if (admin == null)
				{
					return BadRequest($"無法找到 ID 為 {adminId} 的 Super Admin");
				}

				// 設置 userid Cookie (有效期 7 天)
				Response.Cookies.Append("userid", adminId.ToString(),
					new CookieOptions
					{
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.Strict,
						Expires = DateTimeOffset.UtcNow.AddDays(7)
					});

				// 設置 admin_role Cookie 標記為 Super Admin (有效期 7 天)
				Response.Cookies.Append("admin_role", "SuperAdmin",
					new CookieOptions
					{
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.Strict,
						Expires = DateTimeOffset.UtcNow.AddDays(7)
					});

				return Ok(new 
				{ 
					message = $"✓ Super Admin Cookie 已設置",
					adminId,
					adminName = admin.Account,
					role = admin.RoleName
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = $"設置 Cookie 失敗: {ex.Message}" });
			}
		}

		/// <summary>
		/// 清除 Super Admin Cookie (測試用途)
		/// </summary>
		[HttpGet]
		public IActionResult ClearSuperAdminCookie()
		{
			Response.Cookies.Delete("userid");
			Response.Cookies.Delete("admin_role");

			return Ok(new { message = "✓ Super Admin Cookie 已清除" });
		}

		/// <summary>
		/// 更新管理員角色
		/// </summary>
		[HttpPost]
		public IActionResult UpdateRole(int adminId, int roleId)
		{
			try
			{
				var currentAdminIdStr = User.FindFirst("userid")?.Value
					?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

				if (!int.TryParse(currentAdminIdStr, out int currentAdminId))
				{
					TempData["Message"] = "無法識別當前登入的管理員 ID";
					return RedirectToAction("Index");
				}

				bool success = _adminService.UpdateAdminRole(adminId, roleId, currentAdminId);
				TempData["Message"] = success ? "管理員角色更新成功" : "更新失敗，請確認管理員 ID 是否存在";
			}
			catch (InvalidOperationException ex)
			{
				TempData["Message"] = ex.Message;
			}
			catch (Exception ex)
			{
				TempData["Message"] = $"更新角色失敗: {ex.Message}";
			}

			return RedirectToAction("Index");
		}
	}
}