namespace ISpanShop.Common.Helpers
{
    public static class StoreStatusHelper
    {
        public static string GetDisplayName(int status) => status switch
        {
            1 => "營業中",
            2 => "休假中",
            3 => "停權",
            _ => "未知"
        };

        public static string GetBadgeClass(int status) => status switch
        {
            1 => "bg-label-success",   // 綠色
            2 => "bg-label-warning",   // 黃色
            3 => "bg-label-danger",    // 紅色
            _ => "bg-label-secondary"
        };
    }
}