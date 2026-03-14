using ISpanShop.Models.DTOs.Common;

namespace ISpanShop.MVC.Areas.Admin.Models.Members
{
    public class LoginLogIndexVm
    {
        public PagedResult<LoginLogItemVm> PagedResult { get; set; }
        public string Keyword { get; set; }
        public string Status { get; set; }
        public int PageSize { get; set; } = 10;
    }

    public class LoginLogItemVm
    {
        public int Id { get; set; }
        public string UserAccount { get; set; }
        public string IpAddress { get; set; }
        public DateTime LoginTime { get; set; }
        public bool IsSuccessful { get; set; }
    }
}
