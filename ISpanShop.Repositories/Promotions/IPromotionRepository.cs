using System.Collections.Generic;
using System.Threading.Tasks;
using ISpanShop.Models.EfModels;

namespace ISpanShop.Repositories.Promotions
{
    /// <summary>活動 Repository 介面（前台唯讀查詢）</summary>
    public interface IPromotionRepository
    {
        /// <summary>[Async] 取得目前進行中的活動（Status==1, StartTime<=now, EndTime>=now, !IsDeleted）</summary>
        /// <param name="promotionType">1=限時特賣, 2=滿額折扣, 3=限量搶購; null=不篩選</param>
        /// <param name="limit">最多回傳幾筆</param>
        Task<IEnumerable<Promotion>> GetActivePromotionsAsync(int? promotionType, int limit);

        /// <summary>[Async] 取得指定賣家的活動列表（分頁）</summary>
        Task<(IEnumerable<Promotion> Items, int TotalCount)> GetSellerPromotionsPagedAsync(
            int sellerId, string? statusFilter, int page, int pageSize);

        /// <summary>[Async] 根據 ID 取得活動（含賣家驗證）</summary>
        Task<Promotion?> GetByIdAsync(int id);

        /// <summary>[Async] 新增活動</summary>
        Task<Promotion> AddPromotionAsync(Promotion promotion);

        /// <summary>[Async] 更新活動</summary>
        Task UpdatePromotionAsync(Promotion promotion);

        /// <summary>[Async] 刪除活動（軟刪除）</summary>
        Task DeletePromotionAsync(int id);
    }
}
