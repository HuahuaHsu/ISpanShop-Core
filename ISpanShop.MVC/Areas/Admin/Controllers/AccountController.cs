using ISpanShop.Models.EfModels;
using ISpanShop.MVC.Areas.Admin.Models.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ISpanShop.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly ISpanShopDBContext _db;

        public AccountController(ISpanShopDBContext db)
        {
            _db = db;
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

            // 查找管理員帳號（Role 為 Admin 的使用者）
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.Account == model.Account &&
                    u.Password == model.Password &&
                    u.IsBlacklisted != true);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "帳號或密碼不正確，請重新輸入。");
                return View(model);
            }

            // 驗證是否為管理員角色
            bool isAdmin = user.Role != null &&
                           (user.Role.RoleName.Contains("Admin") ||
                            user.Role.RoleName.Contains("admin") ||
                            user.Role.RoleName == "Administrator");

            if (!isAdmin)
            {
                ModelState.AddModelError(string.Empty, "您的帳號沒有後台管理權限。");
                return View(model);
            }

            // 建立 Claims 並登入
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Account),
                new Claim(ClaimTypes.Role, user.Role.RoleName),
                new Claim("UserId", user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
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
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account", new { area = "Admin" });
        }
    }
}
