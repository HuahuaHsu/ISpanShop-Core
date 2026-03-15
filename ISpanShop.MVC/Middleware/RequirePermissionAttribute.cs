using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ISpanShop.Common.Helpers;

namespace ISpanShop.MVC.Middleware
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _permissionKey;

        public RequirePermissionAttribute(string permissionKey)
        {
            _permissionKey = permissionKey;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // 1. 未登入 -> 導向登入頁
            if (user == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", new { area = "Admin" });
                return;
            }

            // 2. 超級管理員擁有所有權限
            if (user.IsSuperAdmin())
            {
                return;
            }

            // 3. 無此權限 -> 導向 AccessDenied 頁
            if (!user.HasPermission(_permissionKey))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", new { area = "Admin" });
            }
        }
    }
}
