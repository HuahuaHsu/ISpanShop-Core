using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ISpanShop.MVC.Models.ViewModels.Reviews
{
    public class ReviewSubmissionVm
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        [Range(1, 5)]
        public byte Rating { get; set; }

        [Required]
        public string Comment { get; set; }

        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
