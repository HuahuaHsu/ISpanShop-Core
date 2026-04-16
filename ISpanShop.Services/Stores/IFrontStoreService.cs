using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Stores;

namespace ISpanShop.Services.Stores
{
    public interface IFrontStoreService
    {
        Task<FrontSellerDashboardDto> GetDashboardDataAsync(int userId);
    }
}
