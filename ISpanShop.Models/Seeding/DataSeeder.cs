using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ISpanShop.Models.EfModels;

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
					var brand = ResolveBrand(dummy.Brand, brands);
					var (productName, productDescription) = TranslateProduct(dummy);
					int basePriceTwd = (int)(dummy.Price * USD_TO_TWD);

					// 建立圖片清單 (可被所有克隆版本共用模板)
					var imageTemplates = BuildImageTemplates(dummy.Images);

					// 資料倍增術：將 1 筆真實商品變種成 5 筆
					for (int k = 0; k < CloneSuffixes.Length; k++)
					{
						var clonePrice = basePriceTwd + (k * 150);
						var variants = ProductVariantHelper.GenerateVariants(dummy.Category, clonePrice, maxCombinations: 6);

						products.Add(CreateProductEntity(
							storeId: store.Id,
							categoryId: category?.Id ?? categories.First().Id,
							brandId: brand?.Id ?? brands.First().Id,
							name: productName + CloneSuffixes[k],
							description: productDescription,
							variants: variants,
							imageTemplates: imageTemplates
						));
					}
				}

				// 3. 將最後 15 筆設為「待審核」
				foreach (var p in products.Skip(Math.Max(0, products.Count - 15)))
				{
					p.Status = 2;
				}

				// 4. 一次性批次寫入資料庫
				context.Products.AddRange(products);
				await context.SaveChangesAsync();
				Console.WriteLine($"✅ 成功匯入 {products.Count} 筆商品 (含多規格變體) 到資料庫");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ 播種過程出錯：{ex.Message}");
				throw;
			}
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
				Status = (byte)_random.Next(0, 2),
				CreatedAt = DateTime.Now.AddDays(-_random.Next(1, 100)),
				UpdatedAt = DateTime.Now,
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

		/// <summary>根據 API 分類名稱找到對應的子分類</summary>
		private static Category ResolveCategory(string apiCategory, List<Category> categories)
		{
			var childCatName = SeederMappings.CategoryHierarchyMap.ContainsKey(apiCategory)
				? SeederMappings.CategoryHierarchyMap[apiCategory].ChildName
				: char.ToUpper(apiCategory[0]) + apiCategory.Substring(1);
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
			var store = context.Stores.FirstOrDefault();
			if (store != null) return store;

			var role = context.Roles.FirstOrDefault();
			if (role == null)
			{
				role = new Role { RoleName = "Seller", Description = "賣家角色" };
				context.Roles.Add(role);
				context.SaveChanges();
			}

			var user = new User
			{
				RoleId = role.Id,
				Account = "dataseed_seller",
				Password = "hashed_password_placeholder",
				Email = "dataseed@example.com",
				IsConfirmed = true,
				IsBlacklisted = false,
				CreatedAt = DateTime.Now,
				UpdatedAt = DateTime.Now
			};
			context.Users.Add(user);
			context.SaveChanges();

			store = new Store
			{
				UserId = user.Id,
				StoreName = "原廠直營",
				Description = "精選商品，品質保證",
				IsVerified = true,
				CreatedAt = DateTime.Now
			};
			context.Stores.Add(store);
			context.SaveChanges();
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

		/// <summary>
		/// 確保資料庫中有預設管理員帳號（admin / Admin@1234）
		/// </summary>
		public static async Task EnsureAdminUserAsync(ISpanShopDBContext context)
		{
			var adminRole = context.Roles.FirstOrDefault(r => r.RoleName == "Admin");
			if (adminRole == null)
			{
				adminRole = new Role { RoleName = "Admin", Description = "後台管理員" };
				context.Roles.Add(adminRole);
				await context.SaveChangesAsync();
				Console.WriteLine("✅ 已建立 Admin 角色");
			}

			var adminUser = context.Users.FirstOrDefault(u => u.Account == "admin");
			if (adminUser == null)
			{
				adminUser = new User
				{
					RoleId = adminRole.Id,
					Account = "admin",
					Password = BCrypt.Net.BCrypt.HashPassword("Admin@1234"),  // ✅ 改這行
					Email = "admin@ispanshop.com",
					IsConfirmed = true,
					IsBlacklisted = false,
					CreatedAt = DateTime.Now,
					UpdatedAt = DateTime.Now
				};
				context.Users.Add(adminUser);
				await context.SaveChangesAsync();
				Console.WriteLine("✅ 已建立預設管理員帳號：admin / Admin@1234");
			}
			else
			{
				// ✅ 帳號已存在，但密碼可能是明碼，自動補上 BCrypt hash
				bool isAlreadyHashed = adminUser.Password != null && adminUser.Password.StartsWith("$2");
				if (!isAlreadyHashed)
				{
					adminUser.Password = BCrypt.Net.BCrypt.HashPassword("Admin@1234");
					await context.SaveChangesAsync();
					Console.WriteLine("✅ 已將 admin 密碼更新為 BCrypt hash");
				}
			}
		}

		/// <summary>
		/// 確保有足夠的待審核商品 (至少 15 筆)
		/// </summary>
		public static async Task EnsurePendingProductsAsync(ISpanShopDBContext context)
		{
			var currentCount = context.Products.Count(p => p.Status == 2);
			if (currentCount >= 15)
			{
				Console.WriteLine($"ℹ️  待審核商品已有 {currentCount} 筆，無需補充。");
				return;
			}

			var needed = 15 - currentCount;
			var candidates = context.Products
				.Where(p => p.Status != 2 && p.IsDeleted != true)
				.Take(needed)
				.ToList();

			foreach (var p in candidates)
			{
				p.Status = 2;
				p.UpdatedAt = DateTime.Now;
			}

			await context.SaveChangesAsync();
			Console.WriteLine($"✅ 已補充 {candidates.Count} 筆待審核商品，目前共 {currentCount + candidates.Count} 筆");
		}
	}
}
