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

			// �o�̪� "DefaultConnection" ������z appsettings.json��appsettings.Development�̪��W�r�@�Ҥ@��
			builder.Services.AddDbContext<ISpanShopDBContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

			//���U���~�A��
			builder.Services.AddScoped<IProductService, ProductService>();

			//���U���~��Ʈw�s��
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

			// �b���ε{���Ұʮɰ����Ƽ���
			using (var scope = app.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				var context = services.GetRequiredService<ISpanShop.Models.EfModels.ISpanShopDBContext>(); // �T�{�A�� DbContext �W��

				// �令�I�s�D�P�B�� SeedAsync
				await ISpanShop.Models.DataSeeder.SeedAsync(context);
				// 每次啟動確保有 15 筆待審核商品（供測試使用）
				await ISpanShop.Models.DataSeeder.EnsurePendingProductsAsync(context);
			}

			app.Run();
		}
	}
}
