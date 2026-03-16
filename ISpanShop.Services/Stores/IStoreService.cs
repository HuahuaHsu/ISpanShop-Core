using ISpanShop.Models.DTOs.Stores;
using System.Collections.Generic;

namespace ISpanShop.Services.Stores
{
    public interface IStoreService
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
        (bool IsSuccess, string Message) ToggleVerified(int storeId, bool isVerified);
        (bool IsSuccess, string Message) ToggleBlacklist(int storeId, bool isBlacklisted);
        (bool IsSuccess, string Message) UpdateStoreStatus(int storeId, int status);
    }
}
