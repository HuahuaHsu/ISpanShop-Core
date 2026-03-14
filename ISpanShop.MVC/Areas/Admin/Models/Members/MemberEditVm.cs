using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISpanShop.MVC.Areas.Admin.Models.Members
{
    public class MemberEditVm
    {
        public int UserId { get; set; }
        
        // ✅ 唯讀欄位：僅供顯示，不參與驗證
        public string? Account { get; set; }
        public string? LevelName { get; set; }
        public decimal TotalSpending { get; set; }
        public int PointBalance { get; set; }
        
        // ✅ 可編輯欄位
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        
        // ✅ 地址欄位（選填）
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? Street { get; set; }
        
        // ✅ 狀態欄位
        public bool IsBlacklisted { get; set; }
        public bool IsSeller { get; set; }

        // ✅ 圖片上傳（選填）
        public IFormFile? AvatarFile { get; set; }

        // ✅ 下拉選單選項（不參與驗證）
        public List<SelectListItem> CityOptions { get; set; } = new();
        public List<SelectListItem> RegionOptions { get; set; } = new();
    }
}
