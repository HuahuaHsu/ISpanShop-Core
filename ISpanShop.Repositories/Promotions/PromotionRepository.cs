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
    }
}
