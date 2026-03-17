using System;
using System.ComponentModel.DataAnnotations;
using ISpanShop.Models.DTOs.Common;

namespace ISpanShop.MVC.Areas.Admin.Models.Points
{
    public class PointHistoryIndexVm //點數首頁模型(含分頁與搜尋)
	{
        public PagedResult<PointHistoryItemVm> PagedResult { get; set; }
        public string Keyword { get; set; }
        public int? UserId { get; set; }
    }

    public class PointHistoryItemVm //點數歷史項目模型
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

    public class PointUpdateVm //點數異動紀錄輸入模型
	{
        [Required(ErrorMessage = "請選擇會員")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "請輸入變動點數")]
        [Range(-100000, 100000, ErrorMessage = "點數範圍需在 -100,000 到 100,000 之間")]
        public int ChangeAmount { get; set; }

        [Display(Name = "異動類型")]
        [Required(ErrorMessage = "請選擇異動類型")]
        public string UpdateType { get; set; } // 加點 / 扣點 / 到期歸零

        [Display(Name = "異動原因")]
        [Required(ErrorMessage = "請選擇異動原因")]
        public string Reason { get; set; }

        [Display(Name = "關聯單號")]
        [Required(ErrorMessage = "請輸入關聯單號")]
        public string OrderNumber { get; set; }

        [Display(Name = "詳細備註")]
        [Required(ErrorMessage = "請輸入詳細備註")]
        public string Remark { get; set; }
    }

    public class BulkPointUpdateVm //批次點數更新模型
	{
        [Required(ErrorMessage = "請輸入變動點數")]
        public int ChangeAmount { get; set; }

        [Required(ErrorMessage = "請選擇異動類型")]
        public string UpdateType { get; set; }

        [Required(ErrorMessage = "請選擇異動原因")]
        public string Reason { get; set; }

        [Required(ErrorMessage = "請輸入關聯單號")]
        public string OrderNumber { get; set; }

        [Required(ErrorMessage = "請輸入詳細備註")]
        public string Remark { get; set; }
    }
}
