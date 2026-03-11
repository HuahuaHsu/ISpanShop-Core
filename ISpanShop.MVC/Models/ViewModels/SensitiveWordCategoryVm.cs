using System.ComponentModel.DataAnnotations;

namespace ISpanShop.MVC.Models.ViewModels
{
    public class SensitiveWordCategoryVm
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "分類名稱為必填")]
        [Display(Name = "分類名稱")]
        [StringLength(50)]
        public string Name { get; set; }

        [Display(Name = "敏感字數量")]
        public int WordCount { get; set; }
    }
}