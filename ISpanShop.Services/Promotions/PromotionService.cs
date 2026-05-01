using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISpanShop.Models.EfModels;
using ISpanShop.Models.DTOs.Products;
using ISpanShop.Repositories.Promotions;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.Services.Promotions
{
    /// <summary>活動 Service（前台唯讀）</summary>
    public class PromotionService
    {
        private readonly IPromotionRepository _repo;
        private readonly ISpanShopDBContext _context;

        public PromotionService(IPromotionRepository repo, ISpanShopDBContext context)
        {
            _repo = repo;
            _context = context;
        }

        /// <summary>
        /// 批量取得商品的進行中活動資訊（每個商品只取第一個活動）
        /// </summary>
        public async Task<Dictionary<int, ProductPromotionInfoDto>> GetActivePromotionsForProductsAsync(List<int> productIds)
        {
            if (productIds == null || productIds.Count == 0)
                return new Dictionary<int, ProductPromotionInfoDto>();

            var now = DateTime.Now;

            var promotionItems = await _context.PromotionItems
                .AsNoTracking()
                .Include(pi => pi.Promotion)
                    .ThenInclude(p => p.PromotionRules)
                .Include(pi => pi.Product)
                    .ThenInclude(p => p.ProductVariants)
                .Where(pi => productIds.Contains(pi.ProductId)
                           && pi.Promotion != null
                           && !pi.Promotion.IsDeleted
                           && pi.Promotion.Status == 1
                           && pi.Promotion.StartTime <= now
                           && pi.Promotion.EndTime >= now
                           && pi.Promotion.Seller.IsBlacklisted != true
                           && !_context.Stores.Any(s => s.UserId == pi.Promotion.SellerId && s.StoreStatus == 3))
                .ToListAsync();

            // 批量查詢所有商品的有庫存最低價
            var minPrices = await _context.ProductVariants
                .Where(v => productIds.Contains(v.ProductId) 
                         && v.IsDeleted != true 
                         && (v.Stock ?? 0) > 0)
                .GroupBy(v => v.ProductId)
                .Select(g => new { ProductId = g.Key, MinPrice = g.Min(v => v.Price) })
                .ToDictionaryAsync(x => x.ProductId, x => x.MinPrice);

            var result = new Dictionary<int, ProductPromotionInfoDto>();

            foreach (var pi in promotionItems)
            {
                if (result.ContainsKey(pi.ProductId)) continue; // 每個商品只取第一個活動

                var promotion = pi.Promotion;
                var rule = promotion.PromotionRules?.FirstOrDefault();

                // 取得有庫存最低價，回退至 OriginalPrice
                var minAvailablePrice = minPrices.ContainsKey(pi.ProductId)
                    ? minPrices[pi.ProductId]
                    : pi.OriginalPrice;

                var discountPrice = pi.DiscountPercent != null && pi.DiscountPercent > 0
                    ? (decimal?)Math.Round(minAvailablePrice * pi.DiscountPercent.Value / 100m, 0)
                    : pi.DiscountPrice;

                result[pi.ProductId] = new ProductPromotionInfoDto
                {
                    PromotionId     = promotion.Id,
                    PromotionName   = promotion.Name,
                    Type            = GetTypeCode(promotion.PromotionType),
                    TypeLabel       = GetTypeLabel(promotion.PromotionType),
                    DiscountPrice   = discountPrice,
                    DiscountPercent = pi.DiscountPercent,
                    OriginalPrice   = minAvailablePrice,
                    EndDate         = promotion.EndTime,
                    Rule            = rule != null ? new PromotionRuleInfoDto
                    {
                        Threshold     = rule.Threshold,
                        DiscountType  = rule.DiscountType,
                        DiscountValue = rule.DiscountValue
                    } : null
                };
            }

            return result;
        }

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
