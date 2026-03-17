using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISpanShop.Repositories.Members.Implementations
{
    public class PointRepository : IPointRepository
    {
        private readonly ISpanShopDBContext _context;

        public PointRepository(ISpanShopDBContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<PointHistory> Items, int TotalCount)> GetPagedPointHistoryAsync(string keyword, int? userId, int page, int pageSize)
        {
            var query = _context.PointHistories
                .Include(ph => ph.User) //Eager Loading（預先載入）
				.ThenInclude(u => u.MemberProfile)
                .AsNoTracking(); //效能優化

			if (userId.HasValue)
            {
                query = query.Where(ph => ph.UserId == userId.Value);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(ph => ph.User.Account.Contains(keyword) || 
                                         ph.User.MemberProfile.FullName.Contains(keyword) ||
                                         ph.OrderNumber.Contains(keyword) ||
                                         ph.Description.Contains(keyword));
            }

            int totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(ph => ph.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
