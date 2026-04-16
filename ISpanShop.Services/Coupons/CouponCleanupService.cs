using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ISpanShop.Services.Coupons;

public class CouponCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<CouponCleanupService> _logger;

    public CouponCleanupService(IServiceProvider services, ILogger<CouponCleanupService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Coupon Cleanup Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoCleanupWork();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing cleanup work.");
            }

            // 每 10 分鐘執行一次
            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }

    private async Task DoCleanupWork()
    {
        using (var scope = _services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ISpanShopDBContext>();
            var now = DateTime.Now;
            var timeout = now.AddMinutes(-30);

            // 1. 找出超過 30 分鐘且狀態為 3 (鎖定中) 的會員優惠券
            var expiredLocks = await context.MemberCoupons
                .Include(mc => mc.Order)
                .Where(mc => mc.UsageStatus == 3 && mc.Order.CreatedAt < timeout && mc.Order.Status == 0)
                .ToListAsync();

            if (expiredLocks.Any())
            {
                _logger.LogInformation($"Found {expiredLocks.Count} expired coupon locks. Releasing...");

                foreach (var mc in expiredLocks)
                {
                    mc.UsageStatus = 0; // 還原為未使用
                    mc.OrderId = null;
                    
                    // 同時處理訂單內鎖定的點數退還
                    var order = mc.Order;
                    if (order != null && order.PointDiscount > 0)
                    {
                        var profile = await context.MemberProfiles.FirstOrDefaultAsync(p => p.UserId == order.UserId);
                        if (profile != null)
                        {
                            profile.PointBalance = (profile.PointBalance ?? 0) + order.PointDiscount;
                            
                            // 記錄點數退還
                            context.PointHistories.Add(new PointHistory
                            {
                                UserId = order.UserId,
                                OrderNumber = order.OrderNumber,
                                ChangeAmount = order.PointDiscount.Value,
                                BalanceAfter = profile.PointBalance.Value,
                                Description = "訂單逾時未付，退還點數",
                                CreatedAt = now
                            });
                        }
                    }
                }

                await context.SaveChangesAsync();
                _logger.LogInformation("Cleanup completed successfully.");
            }
        }
    }
}
