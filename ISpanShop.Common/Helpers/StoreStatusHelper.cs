namespace ISpanShop.Common.Helpers
{
    public static class StoreStatusHelper
    {
        public static string GetDisplayName(int status, bool? isVerified = true, bool isBlacklisted = false)
        {
            if (isBlacklisted) return "已封鎖";
            if (isVerified == null) return "待審核";
            if (isVerified == false) return "審核退回";

            return status switch
            {
                1 => "營業中",
                2 => "休假中",
                3 => "停權",
                _ => "未知"
            };
        }

        public static string GetBadgeClass(int status, bool? isVerified = true, bool isBlacklisted = false)
        {
            if (isBlacklisted) return "bg-label-danger";
            if (isVerified == null) return "bg-label-warning";
            if (isVerified == false) return "bg-label-danger";

            return status switch
            {
                1 => "bg-label-success",   // 綠色
                2 => "bg-label-warning",   // 黃色
                3 => "bg-label-danger",    // 紅色
                _ => "bg-label-secondary"
            };
        }
    }
}