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
//GetPagedPointHistoryAsync（非同步取得分頁的點數歷史紀錄）。