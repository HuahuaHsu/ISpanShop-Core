using System.Threading.Tasks;
using ISpanShop.Common.Enums;
using ISpanShop.Models.DTOs.Stores;
using ISpanShop.Models.EfModels;
using ISpanShop.Models.DTOs.Orders;
using System.Collections.Generic;

namespace ISpanShop.Services.Stores
{
    public interface IFrontStoreService
    {
        Task<FrontSellerDashboardDto> GetDashboardDataAsync(int userId, int days = 7);
        Task<bool> ApplyStoreAsync(int userId, StoreApplyRequestDto dto);
        Task<string> GetStoreStatusAsync(int userId); // 回傳狀態：NotApplied, Pending, Approved, Rejected
        Task<(string Status, bool IsBanned)> GetStoreStatusDetailAsync(int userId);
        Task<UpdateStoreInfoRequestDto> GetStoreInfoAsync(int userId);
        Task<Store?> GetStoreByUserIdAsync(int userId);
        Task<bool> UpdateStoreInfoAsync(int userId, UpdateStoreInfoRequestDto dto);
        Task<int> GetPendingOrdersCountAsync(int userId);
		Task<StorePublicProfileDto?> GetPublicStoreProfileAsync(int storeId);

        // 賣家訂單管理 (支援分頁)
        Task<PagedResultDto<SellerOrderListDto>> GetSellerOrdersAsync(int userId, OrderStatus? status = null, int page = 1, int pageSize = 10, string keyword = null);
        Task<SellerOrderDetailDto> GetSellerOrderDetailAsync(int userId, long orderId);
        Task<bool> UpdateOrderStatusAsync(int userId, long orderId, OrderStatus newStatus);

        // 賣家退貨管理
        Task<PagedResultDto<SellerReturnListDto>> GetSellerReturnsAsync(int userId, bool? isProcessed = null, int page = 1, int pageSize = 10);
        Task<SellerReturnDetailDto> GetSellerReturnDetailAsync(int userId, long orderId);
        Task<bool> ReviewReturnRequestAsync(int userId, long orderId, ReviewReturnRequestDto dto);

        // 賣家評價回覆
        Task<bool> ReplyToReviewAsync(int userId, SellerReplyDto dto);
	}
}
