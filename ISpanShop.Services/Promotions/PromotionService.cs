using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Promotions;

namespace ISpanShop.Services.Promotions
{
    /// <summary>活動 Service（前台唯讀）</summary>
    public class PromotionService
    {
        private readonly IPromotionRepository _repo;

        public PromotionService(IPromotionRepository repo) => _repo = repo;

        /// <summary>
        /// 取得目前進行中的活動。
        /// </summary>
        /// <param name="type">英文代號：flashSale / discount / limitedBuy；null 或空字串 = 不篩選</param>
        /// <param name="limit">最多幾筆（上限 20）</param>
        public async Task<IEnumerable<Promotion>> GetActivePromotionsAsync(string? type, int limit)
        {
            limit = Math.Clamp(limit, 1, 20);

            int? typeInt = type?.ToLowerInvariant() switch
            {
                "flashsale"  => 1,
                "discount"   => 2,
                "limitedbuy" => 3,
                _            => null
            };

            return await _repo.GetActivePromotionsAsync(typeInt, limit);
        }

        // ── 靜態對照表 ──────────────────────────────────────────────

        public static string GetTypeCode(int promotionType) => promotionType switch
        {
            1 => "flashSale",
            2 => "discount",
            3 => "limitedBuy",
            _ => "other"
        };

        public static string GetTypeLabel(int promotionType) => promotionType switch
        {
            1 => "限時特賣",
            2 => "滿額折扣",
            3 => "限量搶購",
            _ => "其他"
        };

        public static string GetStatusText(int status, DateTime startTime, DateTime endTime)
        {
            var now = DateTime.Now;
            return status switch
            {
                0 => "待審核",
                1 when startTime > now => "即將開始",
                1 when endTime < now => "已結束",
                1 => "進行中",
                2 => "已拒絕",
                3 => "已結束",
                _ => "未知"
            };
        }

        /// <summary>取得賣家活動列表（分頁）</summary>
        public async Task<(IEnumerable<Promotion> Items, int TotalCount)> GetSellerPromotionsAsync(
            int sellerId, string? statusFilter, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);
            return await _repo.GetSellerPromotionsPagedAsync(sellerId, statusFilter, page, pageSize);
        }

        /// <summary>取得單一活動（含賣家驗證）</summary>
        public async Task<Promotion?> GetPromotionByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        /// <summary>新增活動</summary>
        public async Task<Promotion> CreatePromotionAsync(Promotion promotion)
        {
            promotion.CreatedAt = DateTime.Now;
            promotion.Status = 0;
            promotion.IsDeleted = false;
            return await _repo.AddPromotionAsync(promotion);
        }

        /// <summary>更新活動</summary>
        public async Task UpdatePromotionAsync(Promotion promotion)
        {
            promotion.UpdatedAt = DateTime.Now;
            await _repo.UpdatePromotionAsync(promotion);
        }

        /// <summary>刪除活動（軟刪除）</summary>
        public async Task DeletePromotionAsync(int id)
        {
            await _repo.DeletePromotionAsync(id);
        }
    }
}
