using ISpanShop.Models.EfModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISpanShop.Repositories.Orders
{
    public interface IOrderReviewRepository
    {
        Task<List<OrderReview>> GetAllAsync();
        Task<OrderReview> GetByIdAsync(int id);
        Task CreateAsync(OrderReview review); // [新增]
        Task UpdateAsync(OrderReview review);
    }
}