using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs;
using ISpanShop.Models.DTOs.Stores;
using ISpanShop.Models.DTOs.Orders;
using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;
using ISpanShop.Common.Enums;

namespace ISpanShop.Services.Stores
{
    public class FrontStoreService : IFrontStoreService
    {
        private readonly ISpanShopDBContext _context;

        public FrontStoreService(ISpanShopDBContext context)
        {
            _context = context;
        }

        public async Task<FrontSellerDashboardDto> GetDashboardDataAsync(int userId, int days = 7)
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (store == null)
            {
                throw new Exception("找不到您的賣場資訊");
            }

            var storeId = store.Id;

            // 0. 計算週期
            var now = DateTime.Today.AddDays(1);
            var currentStart = DateTime.Today.AddDays(-(days - 1));
            var prevStart = currentStart.AddDays(-days);

            // 當期與上期的訂單數 (用於成長率)
            var ordersCurrent = await _context.Orders
                .CountAsync(o => o.StoreId == storeId && o.CreatedAt >= currentStart && o.CreatedAt < now);
            var ordersPrev = await _context.Orders
                .CountAsync(o => o.StoreId == storeId && o.CreatedAt >= prevStart && o.CreatedAt < currentStart);

            // 當期與上期的營收 (已完成訂單)
            var revenueCurrent = await _context.Orders
                .Where(o => o.StoreId == storeId && o.CreatedAt >= currentStart && o.CreatedAt < now && o.Status == (byte)OrderStatus.Completed)
                .SumAsync(o => o.FinalAmount);
            var revenuePrev = await _context.Orders
                .Where(o => o.StoreId == storeId && o.CreatedAt >= prevStart && o.CreatedAt < currentStart && o.Status == (byte)OrderStatus.Completed)
                .SumAsync(o => o.FinalAmount);

            string orderGrowthRateStr = "0%";
            string orderGrowthType = "neutral";
            if (ordersPrev == 0)
            {
                if (ordersCurrent > 0) { orderGrowthRateStr = "100%"; orderGrowthType = "up"; }
            }
            else
            {
                double rate = (double)(ordersCurrent - ordersPrev) / ordersPrev;
                orderGrowthRateStr = Math.Abs(rate).ToString("P0");
                orderGrowthType = rate > 0 ? "up" : (rate < 0 ? "down" : "neutral");
            }

            string revGrowthRateStr = "0%";
            string revGrowthType = "neutral";
            if (revenuePrev == 0)
            {
                if (revenueCurrent > 0) { revGrowthRateStr = "100%"; revGrowthType = "up"; }
            }
            else
            {
                decimal rate = (revenueCurrent - revenuePrev) / revenuePrev;
                revGrowthRateStr = Math.Abs(rate).ToString("P0");
                revGrowthType = rate > 0 ? "up" : (rate < 0 ? "down" : "neutral");
            }

            // 1. 取得 KPI
            var kpis = new SellerKpiDto
            {
                TotalRevenue = await _context.Orders
                    .Where(o => o.StoreId == storeId && o.Status == (byte)OrderStatus.Completed)
                    .SumAsync(o => o.FinalAmount),

                RevenueLast7Days = revenueCurrent, // 雖然變數名沒改，但回傳的是當期(days)營收
                RevenueGrowthRate = revGrowthRateStr,
                RevenueGrowthType = revGrowthType,

                TotalOrders = await _context.Orders
                    .CountAsync(o => o.StoreId == storeId),

                OrdersLast7Days = ordersCurrent,
                OrdersGrowthRate = orderGrowthRateStr,
                OrdersGrowthType = orderGrowthType,

                PendingOrders = await _context.Orders
                    .CountAsync(o => o.StoreId == storeId && o.Status == (byte)OrderStatus.Processing),

                PendingRefundCount = await _context.Orders
                    .CountAsync(o => o.StoreId == storeId && o.Status == (byte)OrderStatus.Returning && o.ReturnRequests.Any(r => r.Status == 0)),

                TotalProducts = await _context.Products
                    .CountAsync(p => p.StoreId == storeId && p.IsDeleted != true),

                LowStockCount = await _context.ProductVariants
                    .CountAsync(v => v.Product.StoreId == storeId && v.IsDeleted != true && v.Stock <= 10),

                TotalViews = await _context.Products
                    .Where(p => p.StoreId == storeId && p.IsDeleted != true)
                    .SumAsync(p => p.ViewCount ?? 0)
            };

            int totalViews = kpis.TotalViews;
            kpis.ConversionRate = totalViews > 0 
                ? ((double)kpis.TotalOrders / totalViews).ToString("P2") 
                : "0.00%";

            // 2. 銷售趨勢 (根據傳入天數動態產生)
            var dailySales = await _context.Orders
                .Where(o => o.StoreId == storeId && o.Status == (byte)OrderStatus.Completed && o.CreatedAt >= currentStart && o.CreatedAt < now)
                .GroupBy(o => o.CreatedAt.Value.Date)
                .Select(g => new { Date = g.Key, Amount = g.Sum(o => o.FinalAmount) })
                .ToListAsync();

            var salesTrend = new ApexChartDataDto();
            var series = new ChartSeriesDto { Name = "營收" };

            for (int i = 0; i < days; i++)
            {
                var date = currentStart.AddDays(i);
                salesTrend.Labels.Add(date.ToString("MM/dd"));
                var dayData = dailySales.FirstOrDefault(d => d.Date == date);
                series.Data.Add(dayData?.Amount ?? 0);
            }
            salesTrend.Series.Add(series);

            // 3. 熱銷商品排行 (過濾該段時間內的銷量)
            var topProductsQuery = await _context.OrderDetails
                .Include(od => od.Order)
                .Where(od => od.Order.StoreId == storeId 
                          && od.Order.Status == (byte)OrderStatus.Completed
                          && od.Order.CreatedAt >= currentStart
                          && od.Order.CreatedAt < now)
                .GroupBy(od => new { od.ProductId, od.ProductName })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    SalesVolume = g.Sum(od => od.Quantity),
                    SalesRevenue = g.Sum(od => (od.Price ?? 0) * od.Quantity)
                })
                .OrderByDescending(p => p.SalesVolume)
                .Take(10)
                .ToListAsync();

            var topProducts = new List<TopProductSalesDto>();
            foreach (var p in topProductsQuery)
            {
                var image = await _context.ProductImages
                    .Where(pi => pi.ProductId == p.ProductId && pi.IsMain == true)
                    .Select(pi => pi.ImageUrl)
                    .FirstOrDefaultAsync() 
                    ?? await _context.ProductImages
                        .Where(pi => pi.ProductId == p.ProductId)
                        .Select(pi => pi.ImageUrl)
                        .FirstOrDefaultAsync();

                topProducts.Add(new TopProductSalesDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductImage = image ?? "",
                    SalesVolume = p.SalesVolume,
                    SalesRevenue = p.SalesRevenue
                });
            }

            // 4. 近期訂單 (前 10 筆)
            var recentOrdersRaw = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .Where(o => o.StoreId == storeId)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .ToListAsync();

            var recentOrders = new List<RecentOrderDto>();
            foreach (var o in recentOrdersRaw)
            {
                var firstDetail = o.OrderDetails.FirstOrDefault();
                string image = firstDetail?.CoverImage;

                if (string.IsNullOrEmpty(image) && firstDetail != null)
                {
                    image = await _context.ProductImages
                        .Where(pi => pi.ProductId == firstDetail.ProductId && pi.IsMain == true)
                        .Select(pi => pi.ImageUrl)
                        .FirstOrDefaultAsync();
                }

                recentOrders.Add(new RecentOrderDto
                {
                    OrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    BuyerName = o.User.Account,
                    ProductName = firstDetail?.ProductName ?? "未知商品",
                    ProductImage = image ?? "",
                    Amount = o.FinalAmount,
                    Status = ((OrderStatus)o.Status).GetDisplayName(),
                    CreatedAt = o.CreatedAt.Value.ToString("yyyy/MM/dd HH:mm")
                });
            }

            return new FrontSellerDashboardDto
            {
                Kpis = kpis,
                SalesTrend = salesTrend,
                TopProducts = topProducts,
                RecentOrders = recentOrders
            };
        }

        public async Task<bool> ApplyStoreAsync(int userId, StoreApplyRequestDto dto)
        {
            var existingStore = await _context.Stores.FirstOrDefaultAsync(s => s.UserId == userId);

            if (existingStore != null)
            {
                if (existingStore.IsVerified == true) throw new Exception("您已經擁有賣場，無需重複申請");
                if (existingStore.IsVerified == null) throw new Exception("您的申請正在審核中，請耐心等候");

                existingStore.StoreName = dto.StoreName;
                existingStore.Description = dto.Description;
                existingStore.LogoUrl = dto.LogoUrl;
                existingStore.IsVerified = null;
                existingStore.StoreStatus = 2;
                existingStore.CreatedAt = DateTime.Now;

                _context.Stores.Update(existingStore);
            }
            else
            {
                var newStore = new Store
                {
                    UserId = userId,
                    StoreName = dto.StoreName,
                    Description = dto.Description,
                    LogoUrl = dto.LogoUrl,
                    IsVerified = null,
                    StoreStatus = 2,
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

            if (store == null) return "NotApplied";
            if (store.IsVerified == null) return "Pending";
            if (store.StoreStatus == 3) return "Suspended";

            return store.IsVerified.Value ? "Approved" : "Rejected";
        }

        public async Task<(string Status, bool IsBanned)> GetStoreStatusDetailAsync(int userId)
        {
            var store = await _context.Stores
                .Include(s => s.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            string status;
            if (store == null) status = "NotApplied";
            else if (store.IsVerified == null) status = "Pending";
            else status = store.IsVerified.Value ? "Approved" : "Rejected";

            bool isBanned = (store?.User?.IsBlacklisted == true) || (store?.StoreStatus == 3);
            return (status, isBanned);
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

        public async Task<Store?> GetStoreByUserIdAsync(int userId)
        {
            return await _context.Stores
                .FirstOrDefaultAsync(s => s.UserId == userId);
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

            return await _context.Orders
                .CountAsync(o => o.StoreId == store.Id && o.Status == (byte)OrderStatus.Processing);
        }

        public async Task<StorePublicProfileDto?> GetPublicStoreProfileAsync(int storeId)
        {
            var store = await _context.Stores
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == storeId);

            if (store == null) return null;

            var productCount = await _context.Products
                .CountAsync(p => p.StoreId == storeId && p.Status == 1 && p.IsDeleted != true);

            return new StorePublicProfileDto
            {
                Id = store.Id,
                Name = store.StoreName ?? string.Empty,
                Description = store.Description,
                LogoUrl = store.LogoUrl,
                Rating = null,
                ProductCount = productCount,
                FollowerCount = 0,
                CreatedAt = store.CreatedAt
            };
        }

        public async Task<PagedResultDto<SellerOrderListDto>> GetSellerOrdersAsync(int userId, OrderStatus? status = null, int page = 1, int pageSize = 10)
        {
            var store = await _context.Stores
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (store == null) throw new Exception("找不到您的賣場");

            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .Where(o => o.StoreId == store.Id);

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == (byte)status.Value);
            }

            var totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = orders.Select(o => {
                var firstDetail = o.OrderDetails.FirstOrDefault();
                string image = firstDetail?.CoverImage;
                
                // 如果訂單明細沒存圖片，去抓商品主圖
                if (string.IsNullOrEmpty(image) && firstDetail != null)
                {
                    image = _context.ProductImages
                        .Where(pi => pi.ProductId == firstDetail.ProductId && pi.IsMain == true)
                        .Select(pi => pi.ImageUrl)
                        .FirstOrDefault();
                }

                // 確保路徑以 / 開頭
                if (!string.IsNullOrEmpty(image) && !image.StartsWith("http") && !image.StartsWith("/"))
                {
                    image = "/" + image;
                }

                return new SellerOrderListDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CreatedAt = o.CreatedAt,
                    FinalAmount = o.FinalAmount,
                    DiscountAmount = o.DiscountAmount,
                    LevelDiscount = o.LevelDiscount,
                    PointDiscount = o.PointDiscount,
                    PromotionDiscount = o.OrderDetails.Sum(od => od.AllocatedDiscountAmount ?? 0),
                    Status = (OrderStatus)o.Status,
                    StatusName = GetStatusName(o.Status),
                    BuyerName = o.User?.Account ?? "未知買家",
                    BuyerId = o.UserId,
                    RecipientName = o.RecipientName,
                    FirstProductName = firstDetail?.ProductName,
                    FirstProductImage = image,
                    TotalItemCount = o.OrderDetails.Sum(od => od.Quantity)
                };
            }).ToList();

            return new PagedResultDto<SellerOrderListDto>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        public async Task<bool> UpdateOrderStatusAsync(int userId, long orderId, OrderStatus newStatus)
        {
            var store = await _context.Stores
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (store == null) throw new Exception("找不到您的賣場");

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.StoreId == store.Id);

            if (order == null) throw new Exception("找不到該筆訂單或該訂單不屬於您的賣場");

            order.Status = (byte)newStatus;

            if (newStatus == OrderStatus.Completed)
            {
                order.CompletedAt = DateTime.Now;
            }

            _context.Orders.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<SellerOrderDetailDto> GetSellerOrderDetailAsync(int userId, long orderId)
        {
            var store = await _context.Stores
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (store == null) throw new Exception("找不到您的賣場");

            var order = await _context.Orders
                .Include(o => o.User)
                    .ThenInclude(u => u.MemberProfile)
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.StoreId == store.Id);

            if (order == null) throw new Exception("找不到該筆訂單或該訂單不屬於您的賣場");

            return new SellerOrderDetailDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CreatedAt = order.CreatedAt ?? DateTime.MinValue,
                PaymentDate = order.PaymentDate,
                CompletedAt = order.CompletedAt,
                Status = (OrderStatus)order.Status,
                StatusName = ((OrderStatus)order.Status).GetDisplayName(),
                
                // 買家資訊
                UserId = order.UserId,
                BuyerAccount = order.User?.Account ?? "未知",
                BuyerName = order.User?.MemberProfile?.FullName ?? order.User?.Account ?? "未知",
                BuyerPhone = order.User?.MemberProfile?.PhoneNumber ?? "未填寫",
                BuyerEmail = order.User?.Email ?? "未填寫",

                // 收件資訊
                RecipientName = order.RecipientName,
                RecipientPhone = order.RecipientPhone,
                RecipientAddress = order.RecipientAddress,
                Note = order.Note,

                // 金額
                TotalAmount = order.TotalAmount,
                ShippingFee = order.ShippingFee,
                DiscountAmount = order.DiscountAmount,
                LevelDiscount = order.LevelDiscount,
                PointDiscount = order.PointDiscount,
                FinalAmount = order.FinalAmount,

                Items = order.OrderDetails.Select(od => {
                    string image = od.CoverImage;
                    if (string.IsNullOrEmpty(image))
                    {
                        image = _context.ProductImages
                            .Where(pi => pi.ProductId == od.ProductId && pi.IsMain == true)
                            .Select(pi => pi.ImageUrl)
                            .FirstOrDefault();
                    }

                    if (!string.IsNullOrEmpty(image) && !image.StartsWith("http") && !image.StartsWith("/"))
                    {
                        image = "/" + image;
                    }

                    return new SellerOrderItemDto
                    {
                        Id = od.Id,
                        ProductId = od.ProductId,
                        VariantId = od.VariantId,
                        ProductName = od.ProductName,
                        VariantName = od.VariantName,
                        SkuCode = od.SkuCode,
                        CoverImage = image,
                        Price = od.Price ?? 0,
                        Quantity = od.Quantity
                    };
                }).ToList()
            };
        }

        public async Task<PagedResultDto<SellerReturnListDto>> GetSellerReturnsAsync(int userId, bool? isProcessed = null, int page = 1, int pageSize = 10)
        {
            var store = await _context.Stores
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (store == null) throw new Exception("找不到您的賣場");

            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.ReturnRequests)
                .Where(o => o.StoreId == store.Id && o.ReturnRequests.Any());

            if (isProcessed.HasValue)
            {
                if (isProcessed.Value)
                {
                    // 已處理：狀態為已退款，或狀態為已完成(代表拒絕後恢復)
                    query = query.Where(o => o.Status == (byte)OrderStatus.Refunded || 
                                           (o.Status == (byte)OrderStatus.Completed && o.ReturnRequests.Any(r => r.Status != 0)));
                }
                else
                {
                    // 待處理：狀態為退貨中
                    query = query.Where(o => o.Status == (byte)OrderStatus.Returning);
                }
            }

            var totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.ReturnRequests.Max(r => r.CreatedAt))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = orders.Select(o => {
                var latestReturn = o.ReturnRequests.OrderByDescending(r => r.CreatedAt).First();
                return new SellerReturnListDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    BuyerName = o.User?.Account ?? "未知買家",
                    RefundAmount = latestReturn.RefundAmount,
                    ReasonCategory = latestReturn.ReasonCategory,
                    CreatedAt = latestReturn.CreatedAt,
                    Status = (OrderStatus)o.Status,
                    StatusName = ((OrderStatus)o.Status).GetDisplayName()
                };
            }).ToList();

            return new PagedResultDto<SellerReturnListDto>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        public async Task<SellerReturnDetailDto> GetSellerReturnDetailAsync(int userId, long orderId)
        {
            var store = await _context.Stores
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (store == null) throw new Exception("找不到您的賣場");

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.ReturnRequests)
                    .ThenInclude(r => r.ReturnRequestImages)
                .Include(o => o.ReturnRequests)
                    .ThenInclude(r => r.ReturnRequestItems)
                        .ThenInclude(ri => ri.OrderDetail)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.StoreId == store.Id);

            if (order == null || !order.ReturnRequests.Any()) throw new Exception("找不到該筆退貨申請");

            var latestReturn = order.ReturnRequests.OrderByDescending(r => r.CreatedAt).First();

            return new SellerReturnDetailDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OrderCreatedAt = order.CreatedAt ?? DateTime.MinValue,
                
                ReasonCategory = latestReturn.ReasonCategory,
                ReasonDescription = latestReturn.ReasonDescription,
                RefundAmount = latestReturn.RefundAmount,
                RequestCreatedAt = latestReturn.CreatedAt,
                ResolvedAt = latestReturn.UpdatedAt,
                Status = (OrderStatus)order.Status,
                StatusName = ((OrderStatus)order.Status).GetDisplayName(),
                
                ImageUrls = latestReturn.ReturnRequestImages.Select(img => img.ImageUrl).ToList(),
                BuyerAccount = order.User?.Account ?? "未知",

                TotalAmount = order.TotalAmount,
                ShippingFee = order.ShippingFee,
                LevelDiscount = order.LevelDiscount,
                DiscountAmount = order.DiscountAmount,
                PointDiscount = order.PointDiscount,
                FinalAmount = order.FinalAmount,

                // 關鍵：這裡只列出「退貨品項」及其數量
                Items = latestReturn.ReturnRequestItems.Select(ri => {
                    string image = ri.OrderDetail.CoverImage;
                    
                    // [優化] 若訂單明細無快照圖，嘗試抓取商品目前的主圖
                    if (string.IsNullOrEmpty(image))
                    {
                        image = _context.ProductImages
                            .Where(pi => pi.ProductId == ri.OrderDetail.ProductId && pi.IsMain == true)
                            .Select(pi => pi.ImageUrl)
                            .FirstOrDefault();
                    }

                    // 確保路徑格式正確
                    if (!string.IsNullOrEmpty(image) && !image.StartsWith("http") && !image.StartsWith("/"))
                    {
                        image = "/" + image;
                    }

                    return new SellerOrderItemDto
                    {
                        Id = ri.OrderDetailId,
                        ProductId = ri.OrderDetail.ProductId,
                        VariantId = ri.OrderDetail.VariantId,
                        ProductName = ri.OrderDetail.ProductName,
                        VariantName = ri.OrderDetail.VariantName,
                        SkuCode = ri.OrderDetail.SkuCode,
                        CoverImage = image,
                        Price = ri.OrderDetail.Price ?? 0,
                        Quantity = ri.Quantity // 這是退貨的數量
                    };
                }).ToList()
            };
        }

        public async Task<bool> ReviewReturnRequestAsync(int userId, long orderId, ReviewReturnRequestDto dto)
        {
            var store = await _context.Stores
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (store == null) throw new Exception("找不到您的賣場");

            var order = await _context.Orders
                .Include(o => o.ReturnRequests)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.StoreId == store.Id);

            if (order == null || order.Status != (byte)OrderStatus.Returning) 
                throw new Exception("該訂單目前不在退貨申請狀態中");

            var latestReturn = order.ReturnRequests.OrderByDescending(r => r.CreatedAt).First();

            if (dto.IsApproved)
            {
                order.Status = (byte)OrderStatus.Refunded;
                latestReturn.Status = 1; // 已同意
            }
            else
            {
                order.Status = (byte)OrderStatus.Completed;
                latestReturn.Status = 2; // 已拒絕
            }

            latestReturn.AdminRemark = dto.Remark;
            latestReturn.UpdatedAt = DateTime.Now;

            _context.Orders.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }

        private string GetStatusName(byte? status)
        {
            return status switch
            {
                0 => "待付款",
                1 => "待出貨",
                2 => "運送中",
                3 => "已完成",
                4 => "已取消",
                5 => "退貨/款中",
                6 => "已退款",
                _ => "未知"
            };
        }
    }
}
