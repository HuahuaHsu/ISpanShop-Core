using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.Models.Seeding
{
    public class DashboardMockSeeder
    {
        private static readonly Random _random = new Random();

        public static async Task SeedAsync(ISpanShopDBContext context)
        {
            // 1. 取得必要基礎資料
            var store = await context.Stores.FirstOrDefaultAsync();
            if (store == null) return;

            var role = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Member") 
                       ?? await context.Roles.FirstOrDefaultAsync();
            
            // 抓取更多商品以增加類別多樣性 (隨機取 200 個變體)
            var products = await context.ProductVariants
                .Include(pv => pv.Product)
                .OrderBy(x => Guid.NewGuid()) 
                .Take(200)
                .ToListAsync();
            
            if (!products.Any()) return;

            // 2. 產生新註冊會員 (近 30 天)
            var newUsers = new List<User>();
            for (int i = 0; i < 20; i++)
            {
                var regDate = DateTime.Now.AddDays(-_random.Next(0, 30)).AddHours(-_random.Next(0, 24));
                newUsers.Add(new User
                {
                    RoleId = role.Id,
                    Account = "mock_user_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    Password = "password_hash",
                    Email = $"mock_{i}_{Guid.NewGuid().ToString("N").Substring(0, 4)}@example.com",
                    IsConfirmed = true,
                    IsBlacklisted = false,
                    CreatedAt = regDate,
                    UpdatedAt = regDate
                });
            }
            context.Users.AddRange(newUsers);
            await context.SaveChangesAsync();

            // 3. 產生訂單與回購行為 (確保有活躍會員與回購率)
            var orders = new List<Order>();
            var allUsers = newUsers.Concat(await context.Users.Take(10).ToListAsync()).ToList();

            foreach (var user in allUsers)
            {
                // 隨機決定這個人下幾單 (1-3單)
                int orderCount = _random.Next(1, 4);
                for (int j = 0; j < orderCount; j++)
                {
                    var createdAt = DateTime.Now.AddDays(-_random.Next(0, 30)).AddHours(-_random.Next(0, 24));
                    var paymentDate = createdAt.AddMinutes(_random.Next(10, 120));
                    
                    // 隨機決定狀態：3 為已完成，這會計入營收與出貨時長
                    byte status = (byte)(_random.Next(0, 10) < 8 ? 3 : 1); 
                    
                    // 如果已完成，設定完成時間 (1-5 天後)
                    DateTime? completedAt = status == 3 ? paymentDate.AddDays(_random.Next(1, 6)).AddHours(_random.Next(0, 24)) : null;

                    var order = new Order
                    {
                        OrderNumber = "MOCK" + DateTime.Now.Ticks.ToString().Substring(10) + _random.Next(100, 999),
                        UserId = user.Id,
                        StoreId = store.Id,
                        Status = status,
                        CreatedAt = createdAt,
                        PaymentDate = paymentDate,
                        CompletedAt = completedAt,
                        RecipientName = user.Account,
                        RecipientPhone = "0912345678",
                        RecipientAddress = "台北市模擬路123號",
                        FinalAmount = 0 // 待會計算
                    };

                    // 增加訂單明細
                    int itemIdx = _random.Next(0, products.Count);
                    var pv = products[itemIdx];
                    int qty = _random.Next(1, 3);
                    order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = pv.ProductId,
                        VariantId = pv.Id,
                        Price = pv.Price,
                        Quantity = qty
                    });
                    
                    order.TotalAmount = pv.Price * qty;
                    order.FinalAmount = order.TotalAmount;
                    orders.Add(order);
                }
            }

            context.Orders.AddRange(orders);
            
            // 4. 製造一些低庫存警告
            var lowStockItems = products.Take(5).ToList();
            foreach (var item in lowStockItems)
            {
                item.Stock = _random.Next(0, 5);
            }

            await context.SaveChangesAsync();
        }
    }
}
