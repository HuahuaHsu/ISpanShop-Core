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
    }
}
