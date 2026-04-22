using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs;
using ISpanShop.Models.DTOs.Stores;
using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.Services.Stores
{
    public class FrontStoreService : IFrontStoreService
    {
        private readonly ISpanShopDBContext _context;

        public FrontStoreService(ISpanShopDBContext context)
        {
            _context = context;
        }

        public async Task<FrontSellerDashboardDto> GetDashboardDataAsync(int userId)
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (store == null)
            {
                throw new Exception("找不到您的賣場資訊");
            }

            var storeId = store.Id;

            // 1. 取得 KPI
            var kpis = new SellerKpiDto
            {
                TotalRevenue = await _context.Orders
                    .Where(o => o.StoreId == storeId && o.Status == 3) // 已完成
                    .SumAsync(o => o.FinalAmount),
                
                TotalOrders = await _context.Orders
                    .CountAsync(o => o.StoreId == storeId),
                
                PendingOrders = await _context.Orders
                    .CountAsync(o => o.StoreId == storeId && o.Status == 1), // 待出貨
                
                TotalProducts = await _context.Products
                    .CountAsync(p => p.StoreId == storeId && p.IsDeleted != true),
                
                LowStockCount = await _context.ProductVariants
                    .CountAsync(v => v.Product.StoreId == storeId && v.IsDeleted != true && v.Stock <= 10) // 假設低於10為低庫存
            };

            // 2. 銷售趨勢 (近 7 日)
            var endDate = DateTime.Today.AddDays(1);
            var startDate = DateTime.Today.AddDays(-6);
            
            var dailySales = await _context.Orders
                .Where(o => o.StoreId == storeId && o.Status == 3 && o.CreatedAt >= startDate && o.CreatedAt < endDate)
                .GroupBy(o => o.CreatedAt.Value.Date)
                .Select(g => new { Date = g.Key, Amount = g.Sum(o => o.FinalAmount) })
                .ToListAsync();

            var salesTrend = new ApexChartDataDto();
            var series = new ChartSeriesDto { Name = "營收" };
            
            for (int i = 0; i < 7; i++)
            {
                var date = startDate.AddDays(i);
                salesTrend.Labels.Add(date.ToString("MM/dd"));
                var dayData = dailySales.FirstOrDefault(d => d.Date == date);
                series.Data.Add(dayData?.Amount ?? 0);
            }
            salesTrend.Series.Add(series);

            // 3. 熱銷商品排行 (前 5 名)
            var topProducts = await _context.OrderDetails
                .Include(od => od.Order)
                .Where(od => od.Order.StoreId == storeId && od.Order.Status == 3)
                .GroupBy(od => od.ProductName)
                .Select(g => new TopProductSalesDto
                {
                    ProductName = g.Key,
                    SalesVolume = g.Sum(od => od.Quantity),
                    SalesRevenue = g.Sum(od => (od.Price ?? 0) * od.Quantity)
                })
                .OrderByDescending(p => p.SalesVolume)
                .Take(5)
                .ToListAsync();

            return new FrontSellerDashboardDto
            {
                Kpis = kpis,
                SalesTrend = salesTrend,
                TopProducts = topProducts
            };
        }

        public async Task<bool> ApplyStoreAsync(int userId, StoreApplyRequestDto dto)
        {
            // 1. 檢查是否已有賣場
            var existingStore = await _context.Stores.FirstOrDefaultAsync(s => s.UserId == userId);
            
            if (existingStore != null)
            {
                // 如果已經審核通過，禁止重新申請
                if (existingStore.IsVerified == true)
                {
                    throw new Exception("您已經擁有賣場，無需重複申請");
                }

                // 如果正在審核中，提示耐心等候
                if (existingStore.IsVerified == null)
                {
                    throw new Exception("您的申請正在審核中，請耐心等候");
                }

                // 如果是被駁回 (IsVerified == false)，則允許覆蓋舊資料並重置為待審核 (null)
                existingStore.StoreName = dto.StoreName;
                existingStore.Description = dto.Description;
                existingStore.LogoUrl = dto.LogoUrl;
                existingStore.IsVerified = null; // 重置為待審核
                existingStore.StoreStatus = 2;   // 重置為休假中
                existingStore.CreatedAt = DateTime.Now; // 更新申請時間

                _context.Stores.Update(existingStore);
            }
            else
            {
                // 2. 建立新賣場 (第一次申請)
                var newStore = new Store
                {
                    UserId = userId,
                    StoreName = dto.StoreName,
                    Description = dto.Description,
                    LogoUrl = dto.LogoUrl,
                    IsVerified = null, // 待審核狀態
                    StoreStatus = 2,    // 預設休假中
                    CreatedAt = DateTime.Now
                };
                _context.Stores.Add(newStore);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<string> GetStoreStatusAsync(int userId)
        {
            var store = await _context.Stores
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (store == null)
            {
                return "NotApplied";
            }

            if (store.IsVerified == null)
            {
                return "Pending";
            }

            return store.IsVerified.Value ? "Approved" : "Rejected";
        }

        public async Task<UpdateStoreInfoRequestDto> GetStoreInfoAsync(int userId)
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (store == null) throw new Exception("找不到賣場資訊");

            return new UpdateStoreInfoRequestDto
            {
                StoreName = store.StoreName,
                Description = store.Description,
                LogoUrl = store.LogoUrl,
                StoreStatus = store.StoreStatus
            };
        }

        public async Task<bool> UpdateStoreInfoAsync(int userId, UpdateStoreInfoRequestDto dto)
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (store == null) throw new Exception("找不到賣場資訊");
            if (store.IsVerified != true) throw new Exception("賣場尚未通過審核或已被停權，無法修改資訊");

            store.StoreName = dto.StoreName;
            store.Description = dto.Description;
            store.LogoUrl = dto.LogoUrl;
            store.StoreStatus = (byte)dto.StoreStatus;

            _context.Stores.Update(store);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> GetPendingOrdersCountAsync(int userId)
        {
            var store = await _context.Stores
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (store == null) return 0;

            // Status 1 為待出貨
            return await _context.Orders
                .CountAsync(o => o.StoreId == store.Id && o.Status == 1);
        }
    }
}
