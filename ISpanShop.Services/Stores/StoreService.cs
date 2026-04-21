using System.Collections.Generic;
using ISpanShop.Models.DTOs.Stores;
using ISpanShop.Repositories.Stores;
using ISpanShop.Repositories.Members;
using ISpanShop.Common.Helpers;

namespace ISpanShop.Services.Stores
{

     public class StoreService : IStoreService
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IMemberRepository _memberRepository;

        public StoreService(IStoreRepository storeRepository, IMemberRepository memberRepository)
        {
            _storeRepository = storeRepository;
            _memberRepository = memberRepository;
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
                store.StoreStatusName = StoreStatusHelper.GetDisplayName(store.StoreStatus, store.IsVerified, store.IsBlacklisted);
                store.StoreStatusBadge = StoreStatusHelper.GetBadgeClass(store.StoreStatus, store.IsVerified, store.IsBlacklisted);
            }

            return stores;
        }

        public (int Verified, int Pending, int Rejected, int Blocked) GetStoreStats()
        {
            return _storeRepository.GetStoreStats();
        }

        public StoreDetailDto? GetStoreById(int storeId)
        {
            var store = _storeRepository.GetStoreById(storeId);
            if (store != null)
            {
                store.StoreStatusName = StoreStatusHelper.GetDisplayName(store.StoreStatus, store.IsVerified, store.IsBlacklisted);
                store.StoreStatusBadge = StoreStatusHelper.GetBadgeClass(store.StoreStatus, store.IsVerified, store.IsBlacklisted);
            }
            return store;
        }

        public (bool IsSuccess, string Message) ToggleVerified(int storeId, bool isVerified)
        {
            var store = _storeRepository.GetStoreById(storeId);
            if (store == null) return (false, "找不到賣場");

            var result = _storeRepository.ToggleVerified(storeId, isVerified);
            if (!result) return (false, "操作失敗");

            // 同步更新會員的身分
            _memberRepository.UpdateIsSeller(store.UserId, isVerified);

            string msg = isVerified ? "已通過審核，賣家身分已開通" : "已取消審核，賣家身分已關閉";
            return (true, msg);
        }

        public (bool IsSuccess, string Message) UpdateStoreStatus(int storeId, int status)
        {
            // 0. 檢查店家是否已通過審核或被封鎖
            var store = _storeRepository.GetStoreById(storeId);
            if (store == null) return (false, "找不到該店家");
            if (store.IsBlacklisted) return (false, "該店主帳號已封鎖，無法變更店家狀態");
            if (store.IsVerified != true) return (false, "店家尚未通過審核，無法變更營業狀態");

            // 1. 確認 storeStatus 在 1~3 之間
            if (status < 1 || status > 3) return (false, "無效的狀態值");

            var result = _storeRepository.UpdateStoreStatus(storeId, status);
            if (!result) return (false, "更新失敗");

            return (true, $"狀態已更新為{StoreStatusHelper.GetDisplayName(status)}");
        }

		public (bool IsSuccess, string Message) ToggleBlacklist(int storeId, bool isBlacklisted)
		{
			var store = _storeRepository.GetStoreById(storeId);
			if (store == null) return (false, "找不到賣場");

			var result = _storeRepository.ToggleBlacklist(store.UserId, isBlacklisted);
			if (!result) return (false, "操作失敗");

			string msg = isBlacklisted ? "已封鎖店主帳號" : "已解除封鎖店主帳號";
			return (true, msg);
		}
	}
}
