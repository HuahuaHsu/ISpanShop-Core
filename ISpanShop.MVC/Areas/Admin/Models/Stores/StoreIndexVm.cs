using ISpanShop.Models.DTOs.Stores;
using System.Collections.Generic;

namespace ISpanShop.MVC.Areas.Admin.Models.Stores
{
    public class StoreIndexVm
    {
        public List<StoreDto> Stores { get; set; } = new();
        public string? Message { get; set; }

        // ── 搜尋條件 ──
        public string? Keyword { get; set; }         // 店家名稱 / 店主帳號
        public string? VerifyStatus { get; set; }    // "all" / "verified" / "unverified"
        public string? BlockStatus { get; set; }     // "all" / "normal" / "blocked"
        public int? StoreStatusFilter { get; set; }  // null = 全部, 1 / 2 / 3

        // ── 排序 ──
        public string SortColumn { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "desc";

        // ── 分頁 ──
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // ── 統計 ──
        public int TotalStores { get; set; }
        public int VerifiedCount { get; set; }
        public int BlockedCount { get; set; }
    }
}