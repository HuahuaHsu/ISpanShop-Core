using ISpanShop.Models.DTOs.Stores;
using System.Collections.Generic;

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
        (int Total, int Verified, int Blocked) GetStoreStats();
        StoreDetailDto? GetStoreById(int storeId);
        bool ToggleVerified(int storeId, bool isVerified);
        bool ToggleBlacklist(int userId, bool isBlacklisted);
        bool UpdateStoreStatus(int storeId, int status);
    }
}
