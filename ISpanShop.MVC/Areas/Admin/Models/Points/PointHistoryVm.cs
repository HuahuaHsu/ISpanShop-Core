using System;
using System.ComponentModel.DataAnnotations;
using ISpanShop.Models.DTOs.Common;

namespace ISpanShop.MVC.Areas.Admin.Models.Points
{
    public class PointHistoryIndexVm
    {
        public PagedResult<PointHistoryItemVm> PagedResult { get; set; }
        public string Keyword { get; set; }
        public int? UserId { get; set; }
    }

    public class PointHistoryItemVm
    {
        public long Id { get; set; }

        public int UserId { get; set; }

        [Display(Name = "會員帳號")]
        public string Account { get; set; }

        [Display(Name = "會員姓名")]
        public string FullName { get; set; }

        [Display(Name = "訂單編號")]
        public string OrderNumber { get; set; }

        [Display(Name = "變動點數")]
        public int ChangeAmount { get; set; }

        [Display(Name = "變動後餘額")]
        public int BalanceAfter { get; set; }

        [Display(Name = "描述")]
        public string Description { get; set; }

        [Display(Name = "建立時間")]
        public DateTime? CreatedAt { get; set; }
    }
}
