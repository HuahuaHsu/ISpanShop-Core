using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Stores;

namespace ISpanShop.Services.Stores
{
    public interface IFrontStoreService
    {
        Task<FrontSellerDashboardDto> GetDashboardDataAsync(int userId);
        Task<bool> ApplyStoreAsync(int userId, StoreApplyRequestDto dto);
        Task<string> GetStoreStatusAsync(int userId); // 回傳狀態：NotApplied, Pending, Approved, Rejected
        Task<UpdateStoreInfoRequestDto> GetStoreInfoAsync(int userId);
        Task<bool> UpdateStoreInfoAsync(int userId, UpdateStoreInfoRequestDto dto);
    }
}
