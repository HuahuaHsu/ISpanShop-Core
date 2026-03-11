using ISpanShop.Models.DTOs;
using ISpanShop.MVC.Models.Admins;
using ISpanShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Controllers
{
	//[Authorize(Roles = "SuperAdmin")] //測試功能階不需要權限
	public class AdminController : Controller
	{
		private readonly IAdminService _adminService;

		public AdminController(IAdminService adminService)
		{
			_adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
		}

		/// <summary>
		/// 管理員列表頁 - 顯示所有管理員及角色更新選項
		/// </summary>
		public IActionResult Index()
		{
			try
			{
				var admins = _adminService.GetAllAdmins().ToList();
				var roles = _adminService.GetAllPermissions().ToList();
				var permissions = _adminService.GetAllPermissions().ToList(); // ← 新增

				var viewModel = new AdminIndexVm
				{
					Admins = admins,
					PermissionOptions = permissions, // ← 新增
					Message = TempData["Message"]?.ToString()
				};
				return View(viewModel);
			}
			catch (Exception ex)
			{
				var emptyVm = new AdminIndexVm
				{
					Admins = new List<AdminDto>(),
					PermissionOptions = new List<AdminPermissionDto>(), // ← 新增
					Message = $"載入管理員列表失敗: {ex.Message}"
				};
				return View(emptyVm);
			}
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
					.FirstOrDefault(a => a.AdminId == adminId && a.RoleName == "SuperAdmin");

				if (admin == null)
				{
					return BadRequest($"無法找到 ID 為 {adminId} 的 Super Admin");
				}

				// 設置 userid Cookie (有效期 7 天)
				Response.Cookies.Append("userid", adminId.ToString(),
					new Microsoft.AspNetCore.Http.CookieOptions
					{
						HttpOnly = true,
						Secure = true,
						SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict,
						Expires = DateTimeOffset.UtcNow.AddDays(7)
					});

				// 設置 admin_role Cookie 標記為 Super Admin (有效期 7 天)
				Response.Cookies.Append("admin_role", "SuperAdmin",
					new Microsoft.AspNetCore.Http.CookieOptions
					{
						HttpOnly = true,
						Secure = true,
						SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict,
						Expires = DateTimeOffset.UtcNow.AddDays(7)
					});

				return Ok(new 
				{ 
					message = $"✓ Super Admin Cookie 已設置",
					adminId = adminId,
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