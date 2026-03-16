using ISpanShop.Models.DTOs.Admins;
using ISpanShop.MVC.Models.Auth;
using ISpanShop.Services.Admins;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ISpanShop.MVC.Areas.Admin.Controllers
{
	[Area("Admin")] //後臺標籤
	public class AuthController : Controller
    {
        private readonly IAdminService _adminService;

        public AuthController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new AdminLoginVm());
        }

        [HttpPost]
        public async Task<IActionResult> Login(AdminLoginVm form)
        {
            if (!ModelState.IsValid) return View(form);

            var remoteIp = HttpContext.Connection.RemoteIpAddress;
            var ipAddress = remoteIp?.ToString();

            // 如果是 IPv6 的回環位址 (::1)，轉換為熟悉的 127.0.0.1
            if (ipAddress == "::1")
            {
                ipAddress = "127.0.0.1";
            }
            else if (remoteIp != null && remoteIp.IsIPv4MappedToIPv6)
            {
                // 如果是 IPv4-mapped IPv6 (例如 ::ffff:127.0.0.1)，則轉為純 IPv4
                ipAddress = remoteIp.MapToIPv4().ToString();
            }

            AdminDto? admin;
            try
            {
                admin = _adminService.VerifyLogin(form.Account, form.Password, ipAddress);
            }
            catch (InvalidOperationException ex) when (ex.Message == "ACCOUNT_BLACKLISTED")
            {
                form.Message = "帳號異常，請聯繫系統管理員。";
                ModelState.AddModelError(string.Empty, form.Message);
                return View(form);
            }

            if (admin == null)
            {
                form.Message = "帳號或密碼錯誤";
                ModelState.AddModelError(string.Empty, form.Message);
                return View(form);
            }

            if (admin.IsFirstLogin)
            {
                // 暫時 Claims (僅包含 ID 用於修改密碼)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, admin.UserId.ToString()),
                    new Claim(ClaimTypes.Name, admin.Account),
                    new Claim("IsFirstLogin", "true")
                };
                var claimsIdentity = new ClaimsIdentity(claims, "AdminCookieAuth");
				var authProperties = new AuthenticationProperties
				{
					IsPersistent = false,
					ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(15)  // 首次登入15分鐘內完成改密碼
				};

				await HttpContext.SignInAsync("AdminCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);
                
                return RedirectToAction("ChangePassword", "Auth", new { area = "Admin" });//絕對位子
            }
            else
            {
                // 1. 查詢該管理員的所有權限
                var adminPerm = _adminService.GetAdminWithPermissions(admin.UserId);

                // 2. 組裝完整 Claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, admin.UserId.ToString()),
                    new Claim(ClaimTypes.Name, admin.Account),
                    new Claim(ClaimTypes.Role, admin.RoleName),
                    new Claim("AdminLevelId", admin.AdminLevelId?.ToString() ?? ""),
                    new Claim("LevelName", adminPerm?.LevelName ?? ""),
                    new Claim("userid", admin.UserId.ToString()) // 為了與現有程式碼相容
                };

                // 3. 將每個 PermissionKey 加入 Claims
                if (adminPerm?.PermissionKeys != null)
                {
                    foreach (var key in adminPerm.PermissionKeys)
                    {
                        claims.Add(new Claim("Permission", key));
                    }
                }

                var claimsIdentity = new ClaimsIdentity(claims, "AdminCookieAuth");
				// 根據 RememberMe 決定 Cookie 是否持久化
				var authProperties = new AuthenticationProperties
				{
					IsPersistent = form.RememberMe,   // true = 關閉瀏覽器後 Cookie 仍存在
					ExpiresUtc = form.RememberMe
						? DateTimeOffset.UtcNow.AddDays(30)   // 記住我：30天
						: DateTimeOffset.UtcNow.AddMinutes(30)  // 不記住：30分鐘過期
				};

				await HttpContext.SignInAsync("AdminCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);
				return RedirectToAction("Dashboard", "Orders", new { area = "Admin" });
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new AdminChangePasswordVm());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(AdminChangePasswordVm form)
        {
            if (!ModelState.IsValid) return View(form);

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login");
            }

            var result = _adminService.ChangePassword(new AdminChangePasswordDto
            {
                UserId = userId,
                NewPassword = form.NewPassword,
                ConfirmPassword = form.ConfirmPassword
            });

            if (result.IsSuccess)
            {
				// 修改成功後，為了取得完整 Claims，通常建議重新登入
				// 重新查詢完整資料
				var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
				var admin = _adminService.VerifyLogin(
					User.FindFirst(ClaimTypes.Name)?.Value ?? "", "", ipAddress);
				// ↑ 無法重新驗證密碼，改用 userId 直接查

				// 簽出舊的暫時 Cookie
				await HttpContext.SignOutAsync("AdminCookieAuth");

				// 提示重新登入
				TempData["Message"] = "密碼修改成功，請重新登入";
				return RedirectToAction("Login", "Auth", new { area = "Admin" });
			}

            form.Message = result.Message;
            ModelState.AddModelError(string.Empty, result.Message);
            return View(form);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AdminCookieAuth");
            return RedirectToAction("Login", "Auth", new { area = "Admin" }); //給予絕對位置
        }
		public IActionResult AccessDenied()
		{
			return View();
		}
	}
}
