using System.Collections.Generic;

namespace ISpanShop.Models.DTOs.Products
{
    /// <summary>
    /// 批次審核 DTO
    /// </summary>
    public class BatchReviewDto
    {
        public List<int> Ids { get; set; } = new List<int>();
    }

    /// <summary>
    /// 批次退回 DTO
    /// </summary>
    public class BatchRejectDto
    {
        public List<int> Ids { get; set; } = new List<int>();
        public string Reason { get; set; } = string.Empty;
    }
}
