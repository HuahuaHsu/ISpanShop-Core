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

			// 這裡的 "DefaultConnection" 必須跟您 appsettings.json或appsettings.Development裡的名字一模一樣
			builder.Services.AddDbContext<ISpanShopDBContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

			//註冊產品服務
			builder.Services.AddScoped<IProductService, ProductService>();

			//註冊產品資料庫存取
			builder.Services.AddScoped<IProductRepository, ProductRepository>();

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

			// 在應用程式啟動時執行資料播種
			using (var scope = app.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				var context = services.GetRequiredService<ISpanShop.Models.EfModels.ISpanShopDBContext>(); // 確認你的 DbContext 名稱

				// 改成呼叫非同步的 SeedAsync
				await ISpanShop.Models.DataSeeder.SeedAsync(context);
			}

			app.Run();
		}
	}
}
