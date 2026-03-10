using ISpanShop.Models.EfModels;
using ISpanShop.MVC.Middleware;
using ISpanShop.Repositories.Products;
using ISpanShop.Repositories.Categories;
using ISpanShop.Repositories.Inventories;
using ISpanShop.Services.Products;
using ISpanShop.Services.Categories;
using ISpanShop.Services.Inventories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ISpanShop.Repositories.Orders;
using ISpanShop.Services.Orders;

namespace ISpanShop.MVC
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllersWithViews();

			// ── Cookie 身份驗證 ──
			builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(options =>
				{
					options.LoginPath = "/Admin/Account/Login";
					options.LogoutPath = "/Admin/Account/Logout";
					options.AccessDeniedPath = "/Admin/Account/Login";
					options.Cookie.Name = "ISpanShop.Admin.Auth";
					options.Cookie.HttpOnly = true;
					options.ExpireTimeSpan = TimeSpan.FromHours(8);
					options.SlidingExpiration = true;
				});

			builder.Services.AddDbContext<ISpanShopDBContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


			builder.Services.AddScoped<IProductService, ProductService>();


			builder.Services.AddScoped<IProductRepository, ProductRepository>();

			builder.Services.AddScoped<ICategorySpecRepository, CategorySpecRepository>();

			builder.Services.AddScoped<CategorySpecService>();

			builder.Services.AddScoped<ICategoryManageRepository, CategoryManageRepository>();
			builder.Services.AddScoped<CategoryManageService>();

			// 註冊倉儲層 (Repositories)
			builder.Services.AddScoped<IOrderRepository, OrderRepository>();

			// 註冊服務層 (Services)
			builder.Services.AddScoped<IOrderService, OrderService>();
			builder.Services.AddScoped<IOrderDashboardService, OrderDashboardService>();


			builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
			builder.Services.AddScoped<IInventoryService, InventoryService>();

			// ── CORS（開發階段允許所有來源，上線前請指定前台網域）──
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("FrontendPolicy", policy =>
				{
					policy.AllowAnyOrigin()
					      .AllowAnyMethod()
					      .AllowAnyHeader();
				});
			});

			// ── Swagger / OpenAPI ──
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo
				{
					Title       = "ISpanShop 前台 API",
					Version     = "v1",
					Description = "ISpanShop 電商平台前台 RESTful API"
				});
			});

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			// ── 全域例外處理（放在 Routing 之後，授權之前）──
			app.UseMiddleware<ExceptionHandlingMiddleware>();

			// ── CORS ──
			app.UseCors("FrontendPolicy");

			// ── Swagger UI（僅開發環境）──
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI(c =>
				{
					c.SwaggerEndpoint("/swagger/v1/swagger.json", "ISpanShop 前台 API v1");
				});
			}

			app.UseAuthentication();
			app.UseAuthorization();

			// ── Area 路由（後台 MVC，必須在 default 之前）──
			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

			// ★ 支援直接以 /Orders/ 做存取，以及讓根目錄預設導向 訂單儀表板
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Orders}/{action=Dashboard}/{id?}",
				defaults: new { area = "Admin" });

			app.MapControllerRoute(
				name: "home",
				pattern: "Home/{action=Index}/{id?}");


			using (var scope = app.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				var context = services.GetRequiredService<ISpanShop.Models.EfModels.ISpanShopDBContext>();


				await ISpanShop.Models.DataSeeder.SeedAsync(context);
				// 補充歷史商品缺少的審核人 / 審核時間（只對 ReviewStatus=1 且 ReviewDate=null 執行一次）
				await ISpanShop.Models.DataSeeder.PatchMissingReviewDataAsync(context);
				// 每次啟動確保有 15 筆待審核商品（供測試使用）
				await ISpanShop.Models.DataSeeder.EnsurePendingProductsAsync(context);
				// 確保後台管理員帳號存在
				await ISpanShop.Models.DataSeeder.EnsureAdminUserAsync(context);

				// 確保 Products 資料表有 IsDeleted 欄位（軟刪除用）
				await context.Database.ExecuteSqlRawAsync(@"
					IF NOT EXISTS (
						SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
						WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'IsDeleted'
					)
					ALTER TABLE Products ADD IsDeleted BIT NOT NULL DEFAULT 0");

				// 確保 Products 審核機制欄位存在（ReviewStatus, ReviewedBy, RejectDate）
				await context.Database.ExecuteSqlRawAsync(@"
					IF NOT EXISTS (
						SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
						WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ReviewStatus'
					)
					ALTER TABLE Products ADD ReviewStatus INT NOT NULL DEFAULT 0");

				await context.Database.ExecuteSqlRawAsync(@"
					IF NOT EXISTS (
						SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
						WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ReviewedBy'
					)
					ALTER TABLE Products ADD ReviewedBy NVARCHAR(100) NULL");

				await context.Database.ExecuteSqlRawAsync(@"
					IF NOT EXISTS (
						SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
						WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ReviewDate'
					)
					ALTER TABLE Products ADD ReviewDate DATETIME NULL");

				// 確保 Categories 資料表有 NameEn 欄位（英文名稱）
				await context.Database.ExecuteSqlRawAsync(@"
					IF NOT EXISTS (
						SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
						WHERE TABLE_NAME = 'Categories' AND COLUMN_NAME = 'NameEn'
					)
					ALTER TABLE Categories ADD NameEn NVARCHAR(200) NULL");

				// 確保 CategorySpecMappings 資料表有 Sort 欄位（分類內屬性排序用）
				await context.Database.ExecuteSqlRawAsync(@"
					IF NOT EXISTS (
						SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
						WHERE TABLE_NAME = 'CategorySpecMappings' AND COLUMN_NAME = 'Sort'
					)
					ALTER TABLE CategorySpecMappings ADD Sort INT NOT NULL DEFAULT 0");

				// 確保 CategorySpecs 資料表有 AllowCustomInput 欄位（允許賣家自填選項）
				await context.Database.ExecuteSqlRawAsync(@"
					IF NOT EXISTS (
						SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
						WHERE TABLE_NAME = 'CategorySpecs' AND COLUMN_NAME = 'AllowCustomInput'
					)
					ALTER TABLE CategorySpecs ADD AllowCustomInput BIT NOT NULL DEFAULT 0");

				// 清除歷史資料中被錯誤加上的 [待審核] 名稱前綴
				await context.Database.ExecuteSqlRawAsync(@"
					UPDATE Products
					SET Name = SUBSTRING(Name, 7, LEN(Name) - 6)
					WHERE LEFT(Name, 6) = N'[待審核] '");
			}

			app.Run();
		}
	}
}
