using System.Linq;
using System.Security.Claims;

namespace ISpanShop.Common.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// 取得目前登入使用者的 ID (從 NameIdentifier Claim 取得)
        /// </summary>
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        /// <summary>
        /// 檢查管理員是否擁有特定權限金鑰 (PermissionKey)
        /// </summary>
        public static bool HasPermission(this ClaimsPrincipal user, string permissionKey)
        {
            if (user == null) return false;

            // 超級管理員 (AdminLevelId = 1) 擁有所有權限
            if (user.IsSuperAdmin()) return true;

            return user.Claims.Any(c =>
                c.Type == "Permission" &&
                c.Value == permissionKey);
        }

        /// <summary>
        /// 檢查是否為超級管理員 (AdminLevelId = 1)
        /// </summary>
        public static bool IsSuperAdmin(this ClaimsPrincipal user)
        {
            if (user == null) return false;

            return user.Claims.Any(c =>
                c.Type == "AdminLevelId" &&
                c.Value == "1");
        }

    }
}
