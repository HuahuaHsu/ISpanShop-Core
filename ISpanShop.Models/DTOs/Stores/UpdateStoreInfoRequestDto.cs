using System;
using System.ComponentModel.DataAnnotations;

namespace ISpanShop.Models.DTOs.Stores
{
    public class UpdateStoreInfoRequestDto
    {
        [Required(ErrorMessage = "賣場名稱為必填")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "賣場名稱需為 2 至 50 個字")]
        public string StoreName { get; set; }

        [StringLength(500, ErrorMessage = "賣場介紹不能超過 500 個字")]
        public string Description { get; set; }

        public string LogoUrl { get; set; }

        [Range(1, 2, ErrorMessage = "無效的營業狀態")]
        public int StoreStatus { get; set; } // 1: 營業中, 2: 休假中
    }
}
