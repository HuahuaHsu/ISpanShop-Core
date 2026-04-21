using System.ComponentModel.DataAnnotations;

namespace ISpanShop.Models.DTOs.Stores
{
    public class StoreApplyRequestDto
    {
        [Required(ErrorMessage = "賣場名稱為必填")]
        [StringLength(50, ErrorMessage = "賣場名稱長度不能超過 50 個字")]
        public string StoreName { get; set; }

        [StringLength(500, ErrorMessage = "賣場描述長度不能超過 500 個字")]
        public string Description { get; set; }

        public string LogoUrl { get; set; }
    }
}
