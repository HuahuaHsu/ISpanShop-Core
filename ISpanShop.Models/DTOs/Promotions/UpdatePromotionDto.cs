using System;
using System.ComponentModel.DataAnnotations;

namespace ISpanShop.Models.DTOs.Promotions
{
    public class UpdatePromotionDto
    {
        [Required(ErrorMessage = "活動名稱為必填")]
        [StringLength(100, ErrorMessage = "活動名稱最多 100 字")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "活動描述最多 500 字")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "活動類型為必填")]
        [Range(1, 3, ErrorMessage = "活動類型必須為 1-3")]
        public int PromotionType { get; set; }

        [Required(ErrorMessage = "開始時間為必填")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "結束時間為必填")]
        public DateTime EndTime { get; set; }

        /// <summary>折扣值（Type1=百分比 off；Type2=折抵金額；Type3=每件折抵金額）</summary>
        [Range(0, 9999999, ErrorMessage = "折扣值不能為負數")]
        public decimal? DiscountValue { get; set; }

        /// <summary>滿額門檻（Type2 滿額折扣用）</summary>
        [Range(0, 9999999, ErrorMessage = "滿額門檻不能為負數")]
        public decimal? MinimumAmount { get; set; }

        /// <summary>限量數量（Type3 限量搶購用，前端顯示用）</summary>
        public int? LimitQuantity { get; set; }
    }
}
