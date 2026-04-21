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
        /// <summary>取得前台公開賣場資訊（含上架商品數）；找不到時回傳 null</summary>
        Task<StorePublicProfileDto?> GetPublicStoreProfileAsync(int storeId);
    }
}
