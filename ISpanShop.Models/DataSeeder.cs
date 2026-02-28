using System;
using System.Collections.Generic;
using System.Linq;
using ISpanShop.Models.EfModels;

namespace ISpanShop.Models
{
    /// <summary>
    /// 電商資料播種程式 - 生成高擬真的商品資料，確保外鍵完整性
    /// </summary>
    public class DataSeeder
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// 商品圖片映射字典 - 綁定商品名稱與 Unsplash 高質量圖片
        /// </summary>
        private static readonly Dictionary<string, string[]> ProductImageMap = new()
        {
            // 3C周邊
            { "iPhone 15 Pro Max", new[] {
                "https://images.unsplash.com/photo-1695048133142-1a20484d2569?w=400",
                "https://images.unsplash.com/photo-1592286927505-1def25115558?w=400"
            }},
            { "Samsung Galaxy S24 Ultra", new[] {
                "https://images.unsplash.com/photo-1511707267537-b85faf00021e?w=400",
                "https://images.unsplash.com/photo-1612198188060-c7c2a3b66eae?w=400"
            }},
            { "iPad Pro 12.9吋", new[] {
                "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400",
                "https://images.unsplash.com/photo-1561070791-2526d30994b5?w=400"
            }},
            { "MacBook Air M3", new[] {
                "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=400",
                "https://images.unsplash.com/photo-1588872657840-790ff3bde4c0?w=400"
            }},
            { "AirPods Max", new[] {
                "https://images.unsplash.com/photo-1613040809024-b4ef7ba99bc3?w=400",
                "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400"
            }},
            { "Sony WH-1000XM5 耳機", new[] {
                "https://images.unsplash.com/photo-1487215078519-e21cc028cb29?w=400",
                "https://images.unsplash.com/photo-1484704849700-f032a568e944?w=400"
            }},
            { "DJI 空拍機 Air 3S", new[] {
                "https://images.unsplash.com/photo-1554224317-b1c5c58a27e2?w=400",
                "https://images.unsplash.com/photo-1473968512647-3e5ec3a7fe05?w=400"
            }},
            { "Anker 65W 快充", new[] {
                "https://images.unsplash.com/photo-1609091839311-d5365f9ff1c5?w=400",
                "https://images.unsplash.com/photo-1632298873484-e73b14c6ea8d?w=400"
            }},

            // 流行服飾
            { "韓版修身T恤", new[] {
                "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=400",
                "https://images.unsplash.com/photo-1551028719-00167b16ebc5?w=400"
            }},
            { "高腰牛仔褲", new[] {
                "https://images.unsplash.com/photo-1541099649105-f69ad21f3246?w=400",
                "https://images.unsplash.com/photo-1542027001643-30ac89d127e8?w=400"
            }},
            { "運動風連帽外套", new[] {
                "https://images.unsplash.com/photo-1556821552-5ff63b1ce3f7?w=400",
                "https://images.unsplash.com/photo-1552886657-c869b3314908?w=400"
            }},
            { "純棉基本款背心", new[] {
                "https://images.unsplash.com/photo-1502921917128-1aa500764cbd?w=400",
                "https://images.unsplash.com/photo-1506629082632-401015062ee3?w=400"
            }},
            { "素色寬鬆襯衫", new[] {
                "https://images.unsplash.com/photo-1594938298603-c8148c4dae35?w=400",
                "https://images.unsplash.com/photo-1551514506-490fedda8914?w=400"
            }},
            { "顯瘦黑色緊身褲", new[] {
                "https://images.unsplash.com/photo-1548080221-e386a6147493?w=400",
                "https://images.unsplash.com/photo-1610619437281-9abc7440199c?w=400"
            }},
            { "潮牌短袖T恤", new[] {
                "https://images.unsplash.com/photo-1577537878838-d5a54c51c254?w=400",
                "https://images.unsplash.com/photo-1537272191477-85db2bda11ac?w=400"
            }},
            { "學院風格紋背心", new[] {
                "https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=400",
                "https://images.unsplash.com/photo-1612120782180-69c5dba89c5d?w=400"
            }},

            // 居家生活
            { "北歐風床上用品組", new[] {
                "https://images.unsplash.com/photo-1552321554-5fefe8c9ef14?w=400",
                "https://images.unsplash.com/photo-1631049307038-da8ec40ed5de?w=400"
            }},
            { "日式木質收納盒", new[] {
                "https://images.unsplash.com/photo-1578500494198-246f612d03b3?w=400",
                "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=400"
            }},
            { "LED智能檯燈", new[] {
                "https://images.unsplash.com/photo-1565636192335-14c2b8ba2e2b?w=400",
                "https://images.unsplash.com/photo-1565363905295-e635b3fec15f?w=400"
            }},
            { "空氣清淨機", new[] {
                "https://images.unsplash.com/photo-1585771724684-38269d6639fd?w=400",
                "https://images.unsplash.com/photo-1585597022615-cd4628902046?w=400"
            }},
            { "高級羽絨被", new[] {
                "https://images.unsplash.com/photo-1578299788035-ef30e0d06d45?w=400",
                "https://images.unsplash.com/photo-1540932239986-310128078ceb?w=400"
            }},
            { "防水浴簾", new[] {
                "https://images.unsplash.com/photo-1552321554-5fefe8c9ef14?w=400",
                "https://images.unsplash.com/photo-1552321782-8c18c8b2f8d0?w=400"
            }},
            { "香氛擴香機", new[] {
                "https://images.unsplash.com/photo-1615621471454-8eb122d47aa1?w=400",
                "https://images.unsplash.com/photo-1606394070240-36e9fd2c0d66?w=400"
            }},
            { "矽膠廚房用具組", new[] {
                "https://images.unsplash.com/photo-1578500494198-246f612d03b3?w=400",
                "https://images.unsplash.com/photo-1578500494198-246f612d03b3?w=400"
            }}
        };

        /// <summary>
        /// 定義商品分類與對應的真實商品名稱庫（簡化版 - 只保留名稱）
        /// </summary>
        private static readonly Dictionary<string, string[]> CategoryProducts = new()
        {
            {
                "3C周邊",
                new[]
                {
                    "iPhone 15 Pro Max",
                    "Samsung Galaxy S24 Ultra",
                    "iPad Pro 12.9吋",
                    "MacBook Air M3",
                    "AirPods Max",
                    "Sony WH-1000XM5 耳機",
                    "DJI 空拍機 Air 3S",
                    "Anker 65W 快充"
                }
            },
            {
                "流行服飾",
                new[]
                {
                    "韓版修身T恤",
                    "高腰牛仔褲",
                    "運動風連帽外套",
                    "純棉基本款背心",
                    "素色寬鬆襯衫",
                    "顯瘦黑色緊身褲",
                    "潮牌短袖T恤",
                    "學院風格紋背心"
                }
            },
            {
                "居家生活",
                new[]
                {
                    "北歐風床上用品組",
                    "日式木質收納盒",
                    "LED智能檯燈",
                    "空氣清淨機",
                    "高級羽絨被",
                    "防水浴簾",
                    "香氛擴香機",
                    "矽膠廚房用具組"
                }
            }
        };

        /// <summary>
        /// 播種資料到資料庫
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        public static void Seed(ISpanShopDBContext context)
        {
            // 避免重複播種
            if (context.Products.Count() >= 100)
            {
                return;
            }

            // ========== 第一步：建立必要的主檔（順序很重要！） ==========

            // 1. 先建立或取得 Role （User 依賴 Role）
            var role = context.Roles.FirstOrDefault();
            if (role == null)
            {
                role = new Role
                {
                    RoleName = "Seller",
                    Description = "賣家角色"
                };
                context.Roles.Add(role);
                context.SaveChanges(); // 保存以取得 Role ID
            }

            // 2. 建立測試 User （Store 依賴 User）
            var user = context.Users.FirstOrDefault(u => u.Account == "testseller");
            if (user == null)
            {
                user = new User
                {
                    RoleId = role.Id,  // ✓ 綁定真實的 Role ID
                    Account = "testseller",
                    Password = "hashed_password_here", // 實際應用應使用密碼雜湊
                    Email = "testseller@example.com",
                    IsConfirmed = true,
                    IsBlacklisted = false,
                    IsSeller = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                context.Users.Add(user);
                context.SaveChanges(); // 保存以取得 User ID
            }

            // 3. 建立或取得 Store （綁定上面建立的 User）
            var store = context.Stores.FirstOrDefault();
            if (store == null)
            {
                store = new Store
                {
                    UserId = user.Id,  // ✓ 綁定真實的 User ID
                    StoreName = "測試商店",
                    Description = "用於測試的主商店",
                    IsVerified = true,
                    CreatedAt = DateTime.Now
                };
                context.Stores.Add(store);
            }

            // 4. 建立或取得 Categories
            var categoryNames = new[] { "3C周邊", "流行服飾", "居家生活", "食品飲料", "美妝保健" };
            var categories = new List<Category>();

            foreach (var catName in categoryNames)
            {
                var category = context.Categories.FirstOrDefault(c => c.Name == catName);
                if (category == null)
                {
                    category = new Category
                    {
                        Name = catName,
                        IconUrl = "https://via.placeholder.com/32x32",
                        Sort = categoryNames.ToList().IndexOf(catName),
                        IsVisible = true
                    };
                    context.Categories.Add(category);
                }
                categories.Add(category);
            }

            // 5. 建立或取得 Brands
            var brandNames = new[] { "品牌A", "品牌B", "品牌C", "品牌D", "品牌E" };
            var brands = new List<Brand>();

            foreach (var brandName in brandNames)
            {
                var brand = context.Brands.FirstOrDefault(b => b.Name == brandName);
                if (brand == null)
                {
                    brand = new Brand
                    {
                        Name = brandName,
                        Description = $"{brandName}旗下商品",
                        LogoUrl = "https://via.placeholder.com/64x64",
                        Sort = Array.IndexOf(brandNames, brandName),
                        IsVisible = true,
                        IsDeleted = false
                    };
                    context.Brands.Add(brand);
                }
                brands.Add(brand);
            }

            // ========== 第二步：第一次 SaveChanges() ==========
            context.SaveChanges();

            // ========== 第三步：抓取真實 ID ==========
            var storeIds = context.Stores.Select(s => s.Id).ToList();
            var categoryIds = context.Categories.Select(c => c.Id).ToList();
            var brandIds = context.Brands.Select(b => b.Id).ToList();

            // ========== 第四步：生成商品並綁定真實 ID ==========
            var products = new List<Product>();
            int productIndex = 1;

            foreach (var categoryEntry in CategoryProducts)
            {
                string categoryName = categoryEntry.Key;
                var productNames = categoryEntry.Value;

                // 獲取該分類的 ID（使用真實 ID）
                int categoryId = context.Categories
                    .FirstOrDefault(c => c.Name == categoryName)?.Id ?? categoryIds.First();

                // 每個分類生成約 33-34 筆資料
                int productsPerCategory = 34;

                for (int i = 0; i < productsPerCategory; i++)
                {
                    // 隨機從該分類的商品名稱中選擇（不添加變數後綴）
                    string productName = productNames[_random.Next(productNames.Length)];

                    // 從真實 ID 集合中隨機挑選
                    int randomStoreId = storeIds[_random.Next(storeIds.Count)];
                    int randomBrandId = brandIds[_random.Next(brandIds.Count)];

                    var product = new Product
                    {
                        StoreId = randomStoreId,  // ✓ 使用真實 Store ID
                        CategoryId = categoryId,  // ✓ 使用真實 Category ID
                        BrandId = randomBrandId,  // ✓ 使用真實 Brand ID
                        Name = productName,
                        Description = $"高品質的 {categoryName} 商品，精心挑選而來。",
                        Status = 1, // 自動通過審核
                        CreatedAt = DateTime.Now.AddDays(_random.Next(-30, 0)),
                        UpdatedAt = DateTime.Now,
                        ProductVariants = GenerateVariants(categoryName),
                        ProductImages = GenerateImages(productName)  // ✓ 使用商品名稱取得 Unsplash 圖片
                    };

                    // 計算最低與最高價
                    if (product.ProductVariants.Count > 0)
                    {
                        product.MinPrice = product.ProductVariants.Min(v => v.Price);
                        product.MaxPrice = product.ProductVariants.Max(v => v.Price);
                    }

                    products.Add(product);
                    productIndex++;
                }
            }

            context.Products.AddRange(products);

            // ========== 第五步：第二次 SaveChanges() ==========
            context.SaveChanges();
        }

        /// <summary>
        /// 根據分類生成規格變體 - 確保每個 SkuCode 唯一
        /// </summary>
        private static List<ProductVariant> GenerateVariants(string categoryName)
        {
            var variants = new List<ProductVariant>();

            if (categoryName == "流行服飾")
            {
                // 服飾類：顏色 × 尺寸組合
                var colors = new[] { "黑", "白", "灰", "藍" };
                var sizes = new[] { "XS", "S", "M", "L", "XL" };

                foreach (var color in colors)
                {
                    foreach (var size in sizes)
                    {
                        variants.Add(new ProductVariant
                        {
                            SkuCode = GenerateUniqueSku(),  // ✓ 生成唯一 SKU
                            VariantName = $"{color} - {size}",
                            SpecValueJson = $"{{\"color\":\"{color}\",\"size\":\"{size}\"}}",
                            Price = Convert.ToDecimal(_random.Next(200, 1500)),
                            Stock = _random.Next(10, 100),
                            SafetyStock = 5,
                            IsDeleted = false
                        });
                    }
                }
            }
            else if (categoryName == "3C周邊")
            {
                var capacities = new[] { "128GB", "256GB", "512GB" };

                foreach (var capacity in capacities)
                {
                    variants.Add(new ProductVariant
                    {
                        SkuCode = GenerateUniqueSku(),  // ✓ 生成唯一 SKU
                        VariantName = capacity,
                        SpecValueJson = $"{{\"capacity\":\"{capacity}\"}}",
                        Price = Convert.ToDecimal(_random.Next(5000, 30000)),
                        Stock = _random.Next(5, 50),
                        SafetyStock = 3,
                        IsDeleted = false
                    });
                }
            }
            else if (categoryName == "居家生活")
            {
                var colors = new[] { "白", "黑", "灰", "米色" };
                var specs = new[] { "標準", "豪華" };

                foreach (var color in colors)
                {
                    foreach (var spec in specs)
                    {
                        variants.Add(new ProductVariant
                        {
                            SkuCode = GenerateUniqueSku(),  // ✓ 生成唯一 SKU
                            VariantName = $"{color} - {spec}",
                            SpecValueJson = $"{{\"color\":\"{color}\",\"spec\":\"{spec}\"}}",
                            Price = Convert.ToDecimal(_random.Next(300, 5000)),
                            Stock = _random.Next(20, 150),
                            SafetyStock = 10,
                            IsDeleted = false
                        });
                    }
                }
            }
            else
            {
                variants.Add(new ProductVariant
                {
                    SkuCode = GenerateUniqueSku(),  // ✓ 生成唯一 SKU
                    VariantName = "標準版",
                    SpecValueJson = "{}",
                    Price = Convert.ToDecimal(_random.Next(100, 5000)),
                    Stock = _random.Next(10, 100),
                    SafetyStock = 5,
                    IsDeleted = false
                });
            }

            return variants;
        }

        /// <summary>
        /// 生成唯一的 SKU 代碼
        /// </summary>
        private static string GenerateUniqueSku()
        {
            // 使用 Guid 的前 8 個字元 + 時間戳記確保唯一性
            string guidPart = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            string timePart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            return $"{guidPart}-{timePart}";
        }

        /// <summary>
        /// 根據商品名稱生成圖片 - 使用 Unsplash 高質量圖片
        /// </summary>
        private static List<ProductImage> GenerateImages(string productName)
        {
            var images = new List<ProductImage>();

            // 從字典中取得該商品對應的圖片 URL
            if (ProductImageMap.TryGetValue(productName, out var imageUrls))
            {
                for (int i = 0; i < imageUrls.Length; i++)
                {
                    images.Add(new ProductImage
                    {
                        ImageUrl = imageUrls[i],
                        IsMain = (i == 0),  // 第一張為主圖
                        SortOrder = i
                    });
                }
            }
            else
            {
                // 若商品不在字典中，使用預設佔位圖
                images.Add(new ProductImage
                {
                    ImageUrl = "https://via.placeholder.com/400x400?text=Product+Image",
                    IsMain = true,
                    SortOrder = 0
                });
            }

            return images;
        }
    }
}
