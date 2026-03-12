using ISpanShop.Models.EfModels;
using ISpanShop.MVC.Areas.Admin.Models.Account;
using ISpanShop.Services.Admins;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Identities
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly IAdminService _adminService;

        public AccountController(IAdminService adminService)
        {
            _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // 已登入直接跳轉
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Dashboard", "Orders", new { area = "Admin" });

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            // 使用 AdminService 驗證登入（支援 BCrypt 密碼驗證）
            var admin = _adminService.VerifyLogin(model.Account, model.Password);

            if (admin == null)
            {
                ModelState.AddModelError(string.Empty, "帳號或密碼不正確，請重新輸入。");
                return View(model);
            }

            // 驗證是否為管理員角色
            if (admin.RoleName != "Admin" && admin.RoleName != "SuperAdmin")
            {
                ModelState.AddModelError(string.Empty, "您的帳號沒有後台管理權限。");
                return View(model);
            }

            // 建立 Claims 並登入
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, admin.UserId.ToString()),
                new Claim(ClaimTypes.Name, admin.Account),
                new Claim(ClaimTypes.Role, admin.RoleName),
                new Claim("userid", admin.UserId.ToString()),
                new Claim("AdminLevel", admin.AdminLevelId?.ToString() ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, "AdminCookieAuth");

            var authProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                "AdminCookieAuth",
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Dashboard", "Orders", new { area = "Admin" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AdminCookieAuth");
            return RedirectToAction("Login", "Account", new { area = "Admin" });
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
