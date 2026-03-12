using ISpanShop.Models.EfModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISpanShop.Repositories.Members
{
    public interface IPointRepository
    {
        Task<(IEnumerable<PointHistory> Items, int TotalCount)> GetPagedPointHistoryAsync(string keyword, int? userId, int page, int pageSize);
    }
}
