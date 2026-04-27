using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Stores;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Stores;
using ISpanShop.Repositories.Members;
using ISpanShop.Common.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.Services.Stores
{
    public class StoreService : IStoreService
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly ISpanShopDBContext _context;

        public StoreService(
            IStoreRepository storeRepository, 
            IMemberRepository memberRepository,
            ISpanShopDBContext context)
        {
            _storeRepository = storeRepository;
            _memberRepository = memberRepository;
            _context = context;
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
            var store = _storeRepository.GetStoreById(storeId);
            if (store == null) return (false, "找不到該店家");
            if (store.IsBlacklisted) return (false, "該店主帳號已封鎖，無法變更店家狀態");
            if (store.IsVerified != true) return (false, "店家尚未通過審核，無法變更營業狀態");

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

        public async Task<SellerTrafficDto> GetTrafficAnalyticsAsync(int storeId)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.StoreId == storeId && !p.IsDeleted)
                .ToListAsync();

            if (!products.Any())
            {
                return new SellerTrafficDto
                {
                    Summary = new TrafficSummaryDto(),
                    TopProducts = new List<TopProductTrafficDto>(),
                    CategoryData = new List<CategoryTrafficDto>()
                };
            }

            int totalViews = products.Sum(p => p.ViewCount ?? 0);
            int totalSales = products.Sum(p => p.TotalSales ?? 0);

            var topProducts = products
                .OrderByDescending(p => p.ViewCount ?? 0)
                .Take(10)
                .Select(p => new TopProductTrafficDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    ProductImage = p.ProductImages?.FirstOrDefault(pi => pi.IsMain == true)?.ImageUrl 
                                ?? p.ProductImages?.FirstOrDefault()?.ImageUrl ?? "",
                    ViewCount = p.ViewCount ?? 0,
                    TotalSales = p.TotalSales ?? 0,
                    ConversionRate = p.ViewCount > 0 
                        ? Math.Round((decimal)(p.TotalSales ?? 0) * 100 / (p.ViewCount ?? 1), 2) 
                        : 0
                }).ToList();

            int top3Views = topProducts.Take(3).Sum(p => p.ViewCount);
            decimal topShare = totalViews > 0 ? Math.Round((decimal)top3Views * 100 / totalViews, 1) : 0;
            decimal avgConv = totalViews > 0 ? Math.Round((decimal)totalSales * 100 / totalViews, 2) : 0;

            var categoryData = products
                .GroupBy(p => p.Category?.Name ?? "未分類")
                .Select(g => new CategoryTrafficDto
                {
                    CategoryName = g.Key,
                    ViewCount = g.Sum(p => p.ViewCount ?? 0),
                    Percentage = totalViews > 0 ? Math.Round((decimal)g.Sum(p => p.ViewCount ?? 0) * 100 / totalViews, 1) : 0
                })
                .OrderByDescending(c => c.ViewCount)
                .ToList();

            return new SellerTrafficDto
            {
                Summary = new TrafficSummaryDto
                {
                    TotalViews = totalViews,
                    TopItemsTrafficShare = topShare,
                    AvgConversionRate = avgConv
                },
                TopProducts = topProducts,
                CategoryData = categoryData
            };
        }
    }
}
