using System.Collections.Generic;
using ISpanShop.Models.DTOs.Stores;
using ISpanShop.Repositories.Stores;
using ISpanShop.Common.Helpers;

namespace ISpanShop.Services.Stores
{

     public class StoreService : IStoreService
    {
        private readonly IStoreRepository _storeRepository;

        public StoreService(IStoreRepository storeRepository)
        {
            _storeRepository = storeRepository;
        }

        public IEnumerable<StoreDto> GetAllStores(
            string? keyword,
            string verifyStatus,
            string blockStatus,
            int? storeStatusFilter,
            string sortColumn,
            string sortDirection,
            int page,
            int pageSize,
            out int totalCount
        )
        {
            var stores = _storeRepository.GetAllStores(
                keyword, verifyStatus, blockStatus, storeStatusFilter, sortColumn, sortDirection, page, pageSize, out totalCount);

            foreach (var store in stores)
            {
                store.StoreStatusName = StoreStatusHelper.GetDisplayName(store.StoreStatus);
                store.StoreStatusBadge = StoreStatusHelper.GetBadgeClass(store.StoreStatus);
            }

            return stores;
        }

        public (int Total, int Verified, int Blocked) GetStoreStats()
        {
            return _storeRepository.GetStoreStats();
        }

        public StoreDetailDto? GetStoreById(int storeId)
        {
            var store = _storeRepository.GetStoreById(storeId);
            if (store != null)
            {
                store.StoreStatusName = StoreStatusHelper.GetDisplayName(store.StoreStatus);
                store.StoreStatusBadge = StoreStatusHelper.GetBadgeClass(store.StoreStatus);
            }
            return store;
        }

        public (bool IsSuccess, string Message) ToggleVerified(int storeId, bool isVerified)
        {
            var result = _storeRepository.ToggleVerified(storeId, isVerified);
            if (!result) return (false, "操作失敗");

            string msg = isVerified ? "已通過審核" : "已取消審核";
            return (true, msg);
        }

        public (bool IsSuccess, string Message) UpdateStoreStatus(int storeId, int status)
        {
            // 1. 確認 storeStatus 在 1~3 之間
            if (status < 1 || status > 3) return (false, "無效的狀態值");

            var result = _storeRepository.UpdateStoreStatus(storeId, status);
            if (!result) return (false, "更新失敗");

            return (true, $"狀態已更新為{StoreStatusHelper.GetDisplayName(status)}");
        }

		public (bool IsSuccess, string Message) ToggleBlacklist(int storeId, bool isBlacklisted)
		{
			throw new NotImplementedException();
		}
	}
}
