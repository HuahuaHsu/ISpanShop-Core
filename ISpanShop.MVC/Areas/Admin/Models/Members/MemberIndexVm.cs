using ISpanShop.Models.DTOs.Common;

namespace ISpanShop.MVC.Areas.Admin.Models.Members
{
    public class MemberIndexVm
    {
        public PagedResult<MemberItemVm> PagedResult { get; set; }
        public string Keyword { get; set; }
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
        public int PointBalance { get; set; }
        public bool IsBlacklisted { get; set; }
        // 地址欄位（供編輯面板讀取與儲存）
        public string City { get; set; } = "";
        public string Region { get; set; } = "";
        public string Street { get; set; } = "";
    }
}
