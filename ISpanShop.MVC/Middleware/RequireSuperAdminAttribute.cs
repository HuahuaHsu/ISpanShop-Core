using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ISpanShop.Common.Helpers;

namespace ISpanShop.MVC.Middleware
{
    /// <summary>
    /// 超級管理員專用權限篩選器 (AdminLevelId = 1)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireSuperAdminAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // 1. 未登入 -> 導向登入頁
            if (user == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", new { area = "Admin" });
                return;
            }

            // 2. 非超級管理員 -> 導向 AccessDenied 頁
            if (!user.IsSuperAdmin())
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", new { area = "Admin" });
            }
        }
    }
}
