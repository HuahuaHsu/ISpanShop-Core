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
    }
}
