using ISpanShop.Models.DTOs.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace ISpanShop.MVC.Areas.Admin.Models.Members
{
    public class MemberIndexVm
    {
        public PagedResult<MemberItemVm> PagedResult { get; set; }
        public string Keyword { get; set; }
        public string Status { get; set; }
        public int? RoleId { get; set; }
        public int? LevelId { get; set; }
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }

        public List<SelectListItem> MembershipLevels { get; set; }
    }

    public class MemberItemVm
    {
        public int UserId { get; set; }
        public string Account { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsSeller { get; set; }
        public string LevelName { get; set; }
        public decimal TotalSpending { get; set; }
        public int PointBalance { get; set; }
        public bool IsBlacklisted { get; set; }
        // 地址欄位（供編輯面板讀取與儲存）
        public string City { get; set; } = "";
        public string Region { get; set; } = "";
        public string Street { get; set; } = "";
    }
}
