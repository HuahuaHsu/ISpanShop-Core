using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.Models.Seeding
{
	/// <summary>
	/// 電商資料播種程式 - 從公開 API (DummyJSON) 串接真實商品資料
	/// ★ 重構版：靜態對照表移至 SeederMappings.cs，規格產生器移至 ProductVariantHelper.cs
	/// ★ 效能優化：批次 SaveChanges，避免迴圈內頻繁存檔
	/// </summary>
	public class DataSeeder
	{
		private static readonly Random _random = new Random();
		private const string DUMMYJSON_URL = "https://dummyjson.com/products?limit=194";
		private const decimal USD_TO_TWD = 30;

		// ====================================================================
		// API 回應的 DTO 定義
		// ====================================================================
		private class DummyJsonResponse
		{
			[JsonPropertyName("products")]
			public List<DummyProduct> Products { get; set; }
		}

		private class DummyProduct
		{
			[JsonPropertyName("title")] public string Title { get; set; }
			[JsonPropertyName("description")] public string Description { get; set; }
			[JsonPropertyName("price")] public decimal Price { get; set; }
			[JsonPropertyName("stock")] public int Stock { get; set; }
			[JsonPropertyName("category")] public string Category { get; set; }
			[JsonPropertyName("brand")] public string Brand { get; set; }
			[JsonPropertyName("images")] public List<string> Images { get; set; } = new();
		}

		// ====================================================================
		// ★ 資料倍增用的商品後綴名
		// ====================================================================
		private static readonly string[] CloneSuffixes =
			{ "", " (2025 全新升級版)", " (特仕限量版)", " (海外平輸版)", " - 聯名精裝版" };

		// ====================================================================
		// CategoryId → StoreId 對應表（依 ReadMe 賣場分類對應）
		// ====================================================================
		private static readonly Dictionary<int, int[]> CategoryToStores = new Dictionary<int, int[]>
		{
			{ 7,  new[] { 1 } },        // 生鮮食材與飲品   → 小明の奇妙雜貨
			{ 8,  new[] { 1, 2, 7 } },  // 居家裝飾與收納   → Store 1, 2, 7 隨機
			{ 9,  new[] { 1, 2 } },     // 廚房餐具與用品   → Store 1, 2 隨機
			{ 5,  new[] { 2 } },        // 大型家具         → 建國五金
			{ 11, new[] { 3 } },        // 筆記型電腦       → 豪客3C
			{ 16, new[] { 3 } },        // 手機與平板周邊   → 豪客3C
			{ 20, new[] { 3 } },        // 智慧型手機       → 豪客3C
			{ 24, new[] { 3 } },        // 平板電腦         → 豪客3C
			{ 13, new[] { 4 } },        // 男款上衣與襯衫   → 秘密衣櫥
			{ 14, new[] { 4 } },        // 男士鞋款         → 秘密衣櫥
			{ 26, new[] { 4 } },        // 女款上衣與洋裝   → 秘密衣櫥
			{ 28, new[] { 4 } },        // 精品包包         → 秘密衣櫥
			{ 29, new[] { 4 } },        // 派對與晚禮服     → 秘密衣櫥
			{ 31, new[] { 4 } },        // 女款鞋類         → 秘密衣櫥
			{ 32, new[] { 4 } },        // 女士腕錶         → 秘密衣櫥
			{ 3,  new[] { 5, 6 } },     // 香水與香氛       → Store 5, 6 隨機
			{ 19, new[] { 5, 6 } },     // 臉部與身體保養   → Store 5, 6 隨機
			{ 2,  new[] { 6 } },        // 彩妝與修容       → 心怡日韓嚴選
			{ 15, new[] { 7 } },        // 男士腕錶         → 宇過天晴文創
			{ 30, new[] { 7 } },        // 珠寶與飾品       → 宇過天晴文創
			{ 22, new[] { 8 } },        // 運動裝備與球類   → 流浪戶外
			{ 23, new[] { 8 } },        // 太陽眼鏡與配件   → 流浪戶外
		};

		// ====================================================================
		// ★★★ 核心播種方法 ★★★
		// ====================================================================
		public static async Task SeedAsync(ISpanShopDBContext context)
		{
			if (context.Products.Any()) return;

			try
			{
				var dummyProducts = await FetchProductsFromApiAsync();
				if (dummyProducts == null || dummyProducts.Count == 0) return;

				// 1. 建立基礎資料 (商店、分類、品牌)
				var store = EnsureStoreExists(context);
				var categories = ExtractAndCreateHierarchyCategories(context, dummyProducts);
				var brands = ExtractAndCreateBrands(context, dummyProducts);

				// 2. 批次產生所有商品
				var products = new List<Product>();

				foreach (var dummy in dummyProducts)
				{
					var category = ResolveCategory(dummy.Category, categories);
					// 分類不在對照表（如 vehicle、motorcycle 汽機車）→ 跳過，不植入資料庫
					if (category == null) continue;

					var brand = ResolveBrand(dummy.Brand, brands);
					var (productName, productDescription) = TranslateProduct(dummy);
					int basePriceTwd = (int)(dummy.Price * USD_TO_TWD);

					// 建立圖片清單 (可被所有克隆版本共用模板)
					var imageTemplates = BuildImageTemplates(dummy.Images);

					// 根據分類決定所屬賣場（找不到對應則退回預設賣場）
					int resolvedStoreId;
					if (CategoryToStores.TryGetValue(category.Id, out var storeOptions))
						resolvedStoreId = storeOptions[_random.Next(storeOptions.Length)];
					else
						resolvedStoreId = store.Id;

					// 資料倍增術：將 1 筆真實商品變種成 5 筆
					for (int k = 0; k < CloneSuffixes.Length; k++)
					{
						var clonePrice = basePriceTwd + (k * 150);
						var variants = ProductVariantHelper.GenerateVariants(dummy.Category, clonePrice, maxCombinations: 6);

						products.Add(CreateProductEntity(
							storeId: resolvedStoreId,
							categoryId: category.Id,
							brandId: brand?.Id ?? brands.First().Id,
							name: productName + CloneSuffixes[k],
							description: productDescription,
							variants: variants,
							imageTemplates: imageTemplates
						));
					}
				}

				// 3. 所有商品預設為「已上架」狀態 (不再生成待審核商品)
				// 備註：若需測試審核流程，請使用「商品總覽」的「生成測試用待審核商品」功能

				// 3b. 庫存狀態平均分佈：1/3 零庫存 / 1/3 低庫存 / 1/3 正常
				//     低庫存閾值由各 variant 的 SafetyStock 決定（系統判斷：Stock <= SafetyStock）
				var allVariants = products
					.SelectMany(p => p.ProductVariants)
					.OrderBy(_ => _random.Next())   // 隨機打散，確保各狀態平均分佈到不同商品
					.ToList();
				int vTotal = allVariants.Count;
				int vThird = vTotal / 3;

				for (int vi = 0; vi < vTotal; vi++)
				{
					var v = allVariants[vi];
					int safety = v.SafetyStock ?? 10;

					if (vi < vThird)
					{
						// 零庫存（已售罄狀態）
						v.Stock = 0;
					}
					else if (vi < vThird * 2)
					{
						// 低庫存警報：Stock 設為 1 ~ SafetyStock，確保 Stock <= SafetyStock 觸發警報
						v.Stock = _random.Next(1, Math.Max(2, safety + 1));
					}
					else
					{
						// 正常庫存：50 ~ 200，且確保高於 SafetyStock（SafetyStock 最高 19，不影響下限 50）
						v.Stock = _random.Next(Math.Max(50, safety + 1), 201);
					}
				}

				// 3c. 上架狀態二次分配：10-20% 已審核通過的商品設為「未上架」
				//     確保商品總覽的「未上架」統計卡片有數據可展示
				var approvedProducts = products
					.OrderBy(_ => _random.Next())   // 隨機打散
					.ToList();
				int unpublishCount = (int)(approvedProducts.Count * _random.Next(10, 21) / 100.0);  // 10-20%

				for (int i = 0; i < unpublishCount; i++)
				{
					approvedProducts[i].Status = 0;  // 未上架（已審核通過但未公開銷售）
				}

				// 4. 一次性批次寫入資料庫
				context.Products.AddRange(products);
				await context.SaveChangesAsync();

				int publishedCount = products.Count(p => p.Status == 1);
				Console.WriteLine($"✅ 成功匯入 {products.Count} 筆商品 (含多規格變體) 到資料庫");
				Console.WriteLine($"   └─ 已上架: {publishedCount} 筆 / 未上架: {unpublishCount} 筆");

				// 5. 順便生成客服工單與評論 (若為空)
				await EnsureSupportTicketsAsync(context);
				await EnsureOrderReviewsAsync(context);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ 播種過程出錯：{ex.Message}");
				throw;
			}
		}

		// ====================================================================
		// ★ 客服工單播種
		// ====================================================================
		public static async Task GenerateSupportTicketsAsync(ISpanShopDBContext context, int count = 20)
		{
			var users = context.Users.Take(10).ToList();
			if (!users.Any()) return;

			var subjects = new[] { "帳號無法登入", "商品收到有瑕疵", "想詢問退貨流程", "檢舉違規賣家", "購物車結帳失敗", "請問這款衣服還會補貨嗎？", "忘記密碼收不到驗證信", "買家提領申請", "檢舉違規商品" };
			var descriptions = new[] { "使用者反應登入時出現 404 錯誤，請工程師檢查 Log", "商品收到時包裝破損，想申請退換貨", "請問退貨運費是由誰負擔？", "這個賣家好像在賣假貨，請查核", "一直跳轉失敗，無法完成付款", "我很喜歡這件，拜託快補貨", "點了好幾次都沒有收到信，垃圾信箱也沒有", "賣家反應無法提領款項", "這個商品描述與圖片不符" };
			
			var tickets = new List<SupportTicket>();
			for (int i = 0; i < count; i++)
			{
				var user = users[_random.Next(users.Count)];
				int status = _random.Next(0, 3); // 0:待處理, 1:處理中, 2:已結案
				int category = _random.Next(0, 3); // 0:訂單, 1:帳號, 2:檢舉

				tickets.Add(new SupportTicket
				{
					UserId = user.Id,
					OrderId = category == 0 ? _random.Next(100, 500) : null,
					Subject = subjects[_random.Next(subjects.Length)],
					Category = (byte)category,
					Description = descriptions[_random.Next(descriptions.Length)],
					Status = (byte)status,
					AdminReply = status == 2 ? "我們已經處理完畢，感謝您的回饋。" : (status == 1 ? "正在查詢相關 Log 中..." : null),
					CreatedAt = DateTime.Now.AddDays(-_random.Next(0, 5)),
					ResolvedAt = status == 2 ? DateTime.Now : null
				});
			}

			context.SupportTickets.AddRange(tickets);
			await context.SaveChangesAsync();
		}

		public static async Task EnsureSupportTicketsAsync(ISpanShopDBContext context)
		{
			if (context.SupportTickets.Any()) return;
			await GenerateSupportTicketsAsync(context, 20);
		}

		// ====================================================================
		// ★ 商品評論播種 (修復：確保 OrderId 存在)
		// ====================================================================
		public static async Task GenerateOrderReviewsAsync(ISpanShopDBContext context, int count = 30)
		{
			var users = context.Users.Take(10).ToList();
			if (!users.Any()) return;

			// 1. 取得真實存在的 Order ID
			var orderIds = context.Orders.Select(o => o.Id).Take(20).ToList();

			// 2. 如果完全沒訂單，先手動生幾筆基本的
			if (!orderIds.Any())
			{
				var store = context.Stores.FirstOrDefault();
				if (store != null)
				{
					for (int j = 0; j < 5; j++)
					{
						var newOrder = new Order
						{
							UserId = users.First().Id,
							StoreId = store.Id,
							OrderNumber = "ORD" + DateTime.Now.ToString("yyyyMMddHHmmss") + j,
							Status = 1,
							TotalAmount = 1000,
							FinalAmount = 1000,
							CreatedAt = DateTime.Now
						};
						context.Orders.Add(newOrder);
					}
					await context.SaveChangesAsync();
					orderIds = context.Orders.Select(o => o.Id).ToList();
				}
			}

			if (!orderIds.Any()) return;

			var comments = new[] { 
				"品質真的很棒，值得推薦！", 
				"發貨速度很快，包裝也很細心。", 
				"這款商品真的很好用，cp值很高。", 
				"雖然有一點色差，但整體來說還不錯。", 
				"這個商品真的很爛，根本是詐騙集團，退錢啦！", 
				"客服態度很好，解決問題很迅速。",
				"這個賣家寄錯東西了，真是差勁！",
				"物超所值，下次還會再買。",
				"外觀精美，手感也很好。",
				"跟照片完全不一樣，感覺被騙了。"
			};

			var reviews = new List<OrderReview>();
			for (int i = 0; i < count; i++)
			{
				var user = users[_random.Next(users.Count)];
				var comment = comments[_random.Next(comments.Length)];
				var orderId = orderIds[_random.Next(orderIds.Count)];

				reviews.Add(new OrderReview
				{
					OrderId = orderId, // 使用真實的 OrderId
					UserId = user.Id,
					Rating = (byte)_random.Next(1, 6),
					Comment = comment,
					IsHidden = false,
					CreatedAt = DateTime.Now.AddDays(-_random.Next(0, 10))
				});
			}

			context.OrderReviews.AddRange(reviews);
			await context.SaveChangesAsync();
		}

		public static async Task EnsureOrderReviewsAsync(ISpanShopDBContext context)
		{
			if (context.OrderReviews.Any()) return;
			await GenerateOrderReviewsAsync(context, 30);
		}

		// ====================================================================
		// ★ 建立單一商品實體 (簡化克隆邏輯)
		// ====================================================================
		private static Product CreateProductEntity(
			int storeId, int categoryId, int brandId,
			string name, string description,
			List<ProductVariant> variants,
			List<(string Url, bool IsMain, int Sort)> imageTemplates)
		{
			return new Product
			{
				StoreId = storeId,
				CategoryId = categoryId,
				BrandId = brandId,
				Name = name,
				Description = description,
				MinPrice = variants.Min(v => v.Price),
				MaxPrice = variants.Max(v => v.Price),
				Status = 1,  // 修改：所有商品預設為「已上架」(1)，不再隨機分配待審核狀態
				ReviewStatus = 1,  // 已審核通過
				ReviewedBy = "系統預設",
				ReviewDate = DateTime.Now.AddDays(-_random.Next(1, 30)),
				CreatedAt = DateTime.Now.AddDays(-_random.Next(1, 100)),
				UpdatedAt = DateTime.Now,
				ViewCount = _random.Next(0, 5001),
				ProductVariants = variants,
				ProductImages = imageTemplates.Select(t => new ProductImage
				{
					ImageUrl = t.Url,
					IsMain = t.IsMain,
					SortOrder = t.Sort
				}).ToList()
			};
		}

		// ====================================================================
		// ★ 輔助方法：解析分類、品牌、翻譯
		// ====================================================================

		/// <summary>根據 API 分類名稱找到對應的子分類；若分類不在對照表則回傳 null</summary>
		private static Category ResolveCategory(string apiCategory, List<Category> categories)
		{
			if (!SeederMappings.CategoryHierarchyMap.ContainsKey(apiCategory))
				return null;
			var childCatName = SeederMappings.CategoryHierarchyMap[apiCategory].ChildName;
			return categories.FirstOrDefault(c => c.Name == childCatName);
		}

		/// <summary>根據原始品牌名稱找到翻譯後的品牌</summary>
		private static Brand ResolveBrand(string rawBrand, List<Brand> brands)
		{
			rawBrand ??= "原廠直營";
			var translatedName = SeederMappings.BrandTranslationMap.ContainsKey(rawBrand)
				? SeederMappings.BrandTranslationMap[rawBrand]
				: rawBrand;
			return brands.FirstOrDefault(b => b.Name == translatedName);
		}

		/// <summary>翻譯商品名稱與描述</summary>
		private static (string Name, string Description) TranslateProduct(DummyProduct dummy)
		{
			if (SeederMappings.ProductTranslationMap.ContainsKey(dummy.Title))
			{
				var t = SeederMappings.ProductTranslationMap[dummy.Title];
				return (t.Title, t.Description);
			}
			return (dummy.Title, string.Format(SeederMappings.FALLBACK_DESCRIPTION_TEMPLATE, dummy.Title, dummy.Description));
		}

		/// <summary>將圖片 URL 轉成可重複使用的模板</summary>
		private static List<(string Url, bool IsMain, int Sort)> BuildImageTemplates(List<string> images)
		{
			if (images == null || images.Count == 0) return new();
			return images.Select((url, i) => (url, i == 0, i)).ToList();
		}

		// ====================================================================
		// ★ 基礎資料建立 (已優化為批次存檔)
		// ====================================================================

		private static async Task<List<DummyProduct>> FetchProductsFromApiAsync()
		{
			using var client = new HttpClient();
			var response = await client.GetAsync(DUMMYJSON_URL);
			response.EnsureSuccessStatusCode();
			var jsonContent = await response.Content.ReadAsStringAsync();
			var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
			var dummyResponse = System.Text.Json.JsonSerializer.Deserialize<DummyJsonResponse>(jsonContent, options);
			return dummyResponse?.Products ?? new List<DummyProduct>();
		}

		private static Store EnsureStoreExists(ISpanShopDBContext context)
		{
			// ★ 防撞車修改：不再自動產生 Role, User, Store，而是檢查雲端有沒有你匯入的資料
			var store = context.Stores.FirstOrDefault();
			if (store == null)
			{
				throw new Exception("❌ 資料庫中找不到任何 Store。請先使用 SSMS 將本機的 Roles, Users, Stores 資料匯入 Azure，再執行播種。");
			}

			var role = context.Roles.FirstOrDefault();
			if (role == null)
			{
				throw new Exception("❌ 資料庫中找不到任何 Role。請先匯入本機資料。");
			}

			// 只要找得到你匯入的 Store，就直接回傳給主程式繼續播種
			return store;
		}
		

		/// <summary>
		/// 批次建立分類階層 (優化：先收集所有新分類，最後統一存檔)
		/// </summary>
		private static List<Category> ExtractAndCreateHierarchyCategories(ISpanShopDBContext context, List<DummyProduct> dummyProducts)
		{
			var flatCategoryList = new List<Category>();
			var apiCategories = dummyProducts.Select(p => p.Category).Distinct().ToList();

			// 第一輪：建立所有父分類
			var parentNames = apiCategories
				.Where(c => SeederMappings.CategoryHierarchyMap.ContainsKey(c))
				.Select(c => SeederMappings.CategoryHierarchyMap[c].ParentName)
				.Distinct()
				.ToList();

			foreach (var parentName in parentNames)
			{
				var parent = context.Categories.FirstOrDefault(c => c.Name == parentName);
				if (parent == null)
				{
					parent = new Category { Name = parentName, Sort = 0, IsVisible = true, ParentId = null };
					context.Categories.Add(parent);
				}
				if (!flatCategoryList.Any(c => c.Name == parentName))
					flatCategoryList.Add(parent);
			}
			context.SaveChanges(); // 父分類需要先拿到 Id

			// 第二輪：批次建立所有子分類
			var newChildren = new List<Category>();
			foreach (var apiCat in apiCategories)
			{
				if (!SeederMappings.CategoryHierarchyMap.ContainsKey(apiCat)) continue;

				var (parentName, childName) = SeederMappings.CategoryHierarchyMap[apiCat];
				var parent = flatCategoryList.First(c => c.Name == parentName);

				var child = context.Categories.FirstOrDefault(c => c.Name == childName);
				if (child == null)
				{
					child = new Category { Name = childName, Sort = 0, IsVisible = true, ParentId = parent.Id };
					newChildren.Add(child);
				}
				if (!flatCategoryList.Any(c => c.Name == childName))
					flatCategoryList.Add(child);
			}

			if (newChildren.Any())
			{
				context.Categories.AddRange(newChildren);
				context.SaveChanges(); // 子分類只存一次
			}

			return flatCategoryList;
		}

		/// <summary>
		/// 批次建立品牌 (優化：收集所有新品牌後一次性 AddRange + SaveChanges)
		/// </summary>
		private static List<Brand> ExtractAndCreateBrands(ISpanShopDBContext context, List<DummyProduct> dummyProducts)
		{
			var rawBrandNames = dummyProducts.Select(p => p.Brand ?? "原廠直營").Distinct().ToList();

			// 一次撈出資料庫中所有已存在的品牌，避免迴圈內反覆查詢
			var existingBrands = context.Brands.ToList();
			var newBrands = new List<Brand>();

			foreach (var rawName in rawBrandNames)
			{
				var translatedName = SeederMappings.BrandTranslationMap.ContainsKey(rawName)
					? SeederMappings.BrandTranslationMap[rawName]
					: rawName;

				var existing = existingBrands.FirstOrDefault(b => b.Name == translatedName);
				if (existing == null && !newBrands.Any(b => b.Name == translatedName))
				{
					newBrands.Add(new Brand
					{
						Name = translatedName,
						Description = $"{translatedName} 官方直營品牌",
						LogoUrl = "https://via.placeholder.com/64x64",
						Sort = 0,
						IsVisible = true,
						IsDeleted = false
					});
				}
			}

			if (newBrands.Any())
			{
				context.Brands.AddRange(newBrands);
				context.SaveChanges(); // 只呼叫一次！
			}

			return context.Brands.ToList();
		}

		// ====================================================================
		// ★ 補丁與初始化方法 (保持不變)
		// ====================================================================

		/// <summary>
		/// 一次性補充歷史商品的審核人 / 審核時間
		/// </summary>
		public static async Task PatchMissingReviewDataAsync(ISpanShopDBContext context)
		{
			var products = context.Products
				.Where(p => p.ReviewStatus == 1 && p.ReviewDate == null && p.IsDeleted != true)
				.ToList();

			if (!products.Any())
			{
				Console.WriteLine("ℹ️  審核資料補充：無需修補，所有已審核商品均有審核時間。");
				return;
			}

			foreach (var product in products)
			{
				var baseTime = product.CreatedAt ?? DateTime.Now.AddDays(-30);
				product.ReviewDate = baseTime.AddMinutes(_random.Next(1, 31));
				if (string.IsNullOrWhiteSpace(product.ReviewedBy))
					product.ReviewedBy = "系統自動審核";
			}

			await context.SaveChangesAsync();
			Console.WriteLine($"✅ 審核資料補充：已修補 {products.Count} 筆商品的審核人 / 審核時間。");
		}

		public static async Task EnsureAdminUserAsync(ISpanShopDBContext context)
		{
			// ★ 防撞車修改：不再自動產生 Admin 角色和帳號
			var adminUser = context.Users.FirstOrDefault(u => u.Account == "admin");
			if (adminUser == null)
			{
				Console.WriteLine("ℹ️  注意：資料庫中找不到預設的 admin 帳號。這沒關係，只要你後續有匯入本機的 User 資料即可。");
			}
			await Task.CompletedTask;
		}

		/// <summary>
		/// [已停用] 確保有足夠的待審核商品 (至少 15 筆)
		/// 修改說明：為了讓系統初始化時「待審核」列表為空，此方法已停用
		/// 若需測試審核流程，請使用「商品總覽」的「生成測試用待審核商品」功能
		/// </summary>
		public static async Task EnsurePendingProductsAsync(ISpanShopDBContext context)
		{
			// 停用此功能：不再自動補充待審核商品
			Console.WriteLine("ℹ️  種子資料不再自動生成待審核商品，待審核列表保持淨空。");
			await Task.CompletedTask;
		}
	}
}