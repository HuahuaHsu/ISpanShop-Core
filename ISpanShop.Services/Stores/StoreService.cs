using System.Collections.Generic;
using ISpanShop.Models.DTOs.Stores;
using ISpanShop.Repositories.Stores;

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
            string sortColumn,
            string sortDirection,
            int page,
            int pageSize,
            out int totalCount
        )
        {
            return _storeRepository.GetAllStores(
                keyword, verifyStatus, blockStatus, sortColumn, sortDirection, page, pageSize, out totalCount);
        }

        public (int Total, int Verified, int Blocked) GetStoreStats()
        {
            return _storeRepository.GetStoreStats();
        }

        public StoreDetailDto? GetStoreById(int storeId)
        {
            return _storeRepository.GetStoreById(storeId);
        }

        public (bool IsSuccess, string Message) ToggleVerified(int storeId, bool isVerified)
        {
            var result = _storeRepository.ToggleVerified(storeId, isVerified);
            if (!result) return (false, "操作失敗");

            string msg = isVerified ? "已通過審核" : "已取消審核";
            return (true, msg);
        }

        public (bool IsSuccess, string Message) ToggleBlacklist(int storeId, bool isBlacklisted)
        {
            // 1. 取得店家詳細資訊以獲得 UserId
            var store = _storeRepository.GetStoreById(storeId);
            if (store == null) return (false, "找不到該店家");

            // 2. 呼叫 Repository 更新 User 表
            var result = _storeRepository.ToggleBlacklist(store.UserId, isBlacklisted);
            if (!result) return (false, "操作失敗");

            string msg = isBlacklisted ? "已封鎖店家" : "已解除封鎖";
            return (true, msg);
        }
    }
}
