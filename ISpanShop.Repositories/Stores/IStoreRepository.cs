using ISpanShop.Models.DTOs.Stores;
using ISpanShop.Models.EfModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISpanShop.Repositories.Stores
{
    public interface IStoreRepository
    {
        IEnumerable<StoreDto> GetAllStores(
            string? keyword,
            string verifyStatus,
            string blockStatus,
            int? storeStatusFilter,
            string sortColumn,
            string sortDirection,
            int page,
            int pageSize,
            out int totalCount
        );
        (int Verified, int Pending, int Rejected, int Blocked) GetStoreStats();
        StoreDetailDto? GetStoreById(int storeId);
        Task<Store?> GetStoreByUserIdAsync(int userId);
        bool ToggleVerified(int storeId, bool isVerified);
        bool ToggleBlacklist(int userId, bool isBlacklisted);
        bool UpdateStoreStatus(int storeId, int status);
    }
}
