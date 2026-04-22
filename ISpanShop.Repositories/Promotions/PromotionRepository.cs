using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.Repositories.Promotions
{
    /// <summary>活動 Repository 實作</summary>
    public class PromotionRepository : IPromotionRepository
    {
        private readonly ISpanShopDBContext _db;

        public PromotionRepository(ISpanShopDBContext db) => _db = db;

        /// <inheritdoc/>
        public async Task<IEnumerable<Promotion>> GetActivePromotionsAsync(int? promotionType, int limit)
        {
            var now = DateTime.Now;

            var query = _db.Promotions
                .AsNoTracking()
                .Where(p => !p.IsDeleted
                         && p.Status == 1
                         && p.StartTime <= now
                         && p.EndTime   >= now);

            if (promotionType.HasValue)
                query = query.Where(p => p.PromotionType == promotionType.Value);

            return await query
                .OrderBy(p => p.EndTime)   // 快到期的排前面
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<(IEnumerable<Promotion> Items, int TotalCount)> GetSellerPromotionsPagedAsync(
            int sellerId, string? statusFilter, int page, int pageSize)
        {
            var query = _db.Promotions
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.SellerId == sellerId);

            var now = DateTime.Now;

            query = statusFilter?.ToLowerInvariant() switch
            {
                "pending" => query.Where(p => p.Status == 0),
                "active" => query.Where(p => p.Status == 1 && p.StartTime <= now && p.EndTime >= now),
                "upcoming" => query.Where(p => p.Status == 1 && p.StartTime > now),
                "rejected" => query.Where(p => p.Status == 2),
                "ended" => query.Where(p => p.Status == 1 && p.EndTime < now),
                _ => query
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(p => p.PromotionRules)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <inheritdoc/>
        public async Task<Promotion?> GetByIdAsync(int id)
        {
            return await _db.Promotions
                .Include(p => p.PromotionItems)
                .Include(p => p.PromotionRules)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        /// <inheritdoc/>
        public async Task<Promotion> AddPromotionAsync(Promotion promotion)
        {
            _db.Promotions.Add(promotion);
            await _db.SaveChangesAsync();
            return promotion;
        }

        /// <inheritdoc/>
        public async Task UpdatePromotionAsync(Promotion promotion)
        {
            _db.Promotions.Update(promotion);
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeletePromotionAsync(int id)
        {
            var promotion = await _db.Promotions.FindAsync(id);
            if (promotion != null)
            {
                promotion.IsDeleted = true;
                promotion.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();
            }
        }
    }
}
