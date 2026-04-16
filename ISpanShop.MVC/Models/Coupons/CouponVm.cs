using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ISpanShop.MVC.Models.Coupons
{
    public class CouponIndexVm
    {
        public List<CouponListItemVm> Items { get; set; } = new();
    }

    public class CouponListItemVm
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string CouponCode { get; set; } = "";
        public string StoreName { get; set; } = "";
        public byte DistributionType { get; set; }
        public byte CouponType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinimumSpend { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalQuantity { get; set; }
        public int UsedQuantity { get; set; }

        public string DistributionTypeName => DistributionType switch
        {
            1 => "公開領取",
            2 => "序號兌換",
            3 => "系統派發",
            _ => "未知"
        };

        public string CouponTypeName => CouponType switch
        {
            1 => "固定金額",
            2 => "百分比",
            _ => "未知"
        };

        public string Status
        {
            get
            {
                var now = DateTime.Now;
                if (now < StartTime) return "即將開始";
                if (now > EndTime) return "已結束";
                if (UsedQuantity >= TotalQuantity) return "已領完";
                return "進行中";
            }
        }
    }

    public class CouponFormVm
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "請選擇商家")]
        public int StoreId { get; set; }

        [Required(ErrorMessage = "請填寫標題")]
        [StringLength(100)]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "請填寫優惠碼")]
        [StringLength(50)]
        public string CouponCode { get; set; } = "";

        public byte DistributionType { get; set; } = 1;

        public byte CouponType { get; set; } = 1;

        [Required(ErrorMessage = "請填寫折扣值")]
        public decimal DiscountValue { get; set; }

        [Required(ErrorMessage = "請填寫最低消費")]
        public decimal MinimumSpend { get; set; }

        public decimal? MaximumDiscount { get; set; }

        [Required(ErrorMessage = "請填寫開始時間")]
        public DateTime StartTime { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "請填寫結束時間")]
        public DateTime EndTime { get; set; } = DateTime.Now.AddDays(7);

        [Required(ErrorMessage = "請填寫發行總數")]
        public int TotalQuantity { get; set; } = 100;

        [Required(ErrorMessage = "請填寫每人領取上限")]
        public int PerUserLimit { get; set; } = 1;

        public bool ApplyToAll { get; set; } = true;
    }
}
