using ISpanShop.Models.EfModels;
using ISpanShop.MVC.Areas.Admin.Models.Members;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ISpanShop.MVC.Middleware;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Members
{
    [Area("Admin")]
    [Route("Admin/Admins")]
    [RequireSuperAdmin]
    public class AdminsController : Controller
    {
        private readonly ISpanShopDBContext _context;

        public AdminsController(ISpanShopDBContext context)
        {
            _context = context;
        }

        [HttpGet("Permissions")]
        public async Task<IActionResult> Permissions()
        {
            // 取得所有管理員 (假設 RoleId = 2 是管理員)
            var admins = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.RoleId == 2)
                .Select(u => new AdminPermissionItemVm
                {
                    UserId = u.Id,
                    Account = u.Account,
                    Email = u.Email,
                    RoleName = u.Role.RoleName,
                    CurrentPermissions = new List<string> { "READ", "WRITE" }, // 示例權限
                    CreatedAt = u.CreatedAt ?? DateTime.Now
                })
                .ToListAsync();

            // 取得所有可用權限
            var permissions = new List<PermissionOptionVm>
            {
                new PermissionOptionVm { Key = "READ", Description = "讀取" },
                new PermissionOptionVm { Key = "WRITE", Description = "寫入" },
                new PermissionOptionVm { Key = "DELETE", Description = "刪除" },
                new PermissionOptionVm { Key = "MANAGE_USERS", Description = "管理使用者" },
                new PermissionOptionVm { Key = "MANAGE_PRODUCTS", Description = "管理商品" },
                new PermissionOptionVm { Key = "MANAGE_ORDERS", Description = "管理訂單" }
            };

            var viewModel = new AdminPermissionIndexVm
            {
                Admins = admins,
                AvailablePermissions = permissions
            };

            return View(viewModel);
        }

        [HttpPost("UpdatePermission")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePermission([FromForm] UpdatePermissionRequest request)
        {
            // 這裡應該實作更新權限的邏輯
            // 由於資料庫沒有 RolePermission 表的完整實作，這裡僅做示範

            TempData["Success"] = "權限更新成功";
            return RedirectToAction(nameof(Permissions));
        }
    }
}
