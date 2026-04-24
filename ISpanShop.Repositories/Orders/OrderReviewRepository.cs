using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISpanShop.Repositories.Orders
{
    public class OrderReviewRepository : IOrderReviewRepository
    {
        private readonly ISpanShopDBContext _context;

        public OrderReviewRepository(ISpanShopDBContext context)
        {
            _context = context;
        }

        public async Task<List<OrderReview>> GetAllAsync()
        {
            return await _context.OrderReviews
                .Include(r => r.ReviewImages)
                .Include(r => r.User)
                    .ThenInclude(u => u.MemberProfile)
                .Include(r => r.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.ProductImages)
                .ToListAsync();
        }

        public async Task<OrderReview> GetByIdAsync(int id)
        {
            return await _context.OrderReviews.FindAsync(id);
        }

        public async Task CreateAsync(OrderReview review)
        {
            _context.OrderReviews.Add(review);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(OrderReview review)
        {
            _context.OrderReviews.Update(review);
            await _context.SaveChangesAsync();
        }
    }
}