using ISpanShop.Models.EfModels;
using ISpanShop.Repositories;
using ISpanShop.Repositories.Interfaces;
using ISpanShop.Services;
using ISpanShop.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.MVC
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllersWithViews();

			builder.Services.AddDbContext<ISpanShopDBContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


			builder.Services.AddScoped<IProductService, ProductService>();


			builder.Services.AddScoped<IProductRepository, ProductRepository>();

			builder.Services.AddScoped<ICategorySpecRepository, CategorySpecRepository>();

			builder.Services.AddScoped<CategorySpecService>();

			builder.Services.AddScoped<ICategoryManageRepository, CategoryManageRepository>();
			builder.Services.AddScoped<CategoryManageService>();

			builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
			builder.Services.AddScoped<IInventoryService, InventoryService>();

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

			app.UseAuthorization();

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");


			using (var scope = app.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				var context = services.GetRequiredService<ISpanShop.Models.EfModels.ISpanShopDBContext>();


				await ISpanShop.Models.DataSeeder.SeedAsync(context);
				// 每次啟動確保有 15 筆待審核商品（供測試使用）
				await ISpanShop.Models.DataSeeder.EnsurePendingProductsAsync(context);

				// 確保 Products 資料表有 IsDeleted 欄位（軟刪除用）
				await context.Database.ExecuteSqlRawAsync(@"
					IF NOT EXISTS (
						SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
						WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'IsDeleted'
					)
					ALTER TABLE Products ADD IsDeleted BIT NOT NULL DEFAULT 0");

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
