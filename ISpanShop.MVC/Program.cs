using ISpanShop.Models.EfModels;
using ISpanShop.Services;
using Microsoft.EntityFrameworkCore;


namespace ISpanShop.MVC
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllersWithViews();

			// 這裡的 "DefaultConnection" 必須跟您 appsettings.json或appsettings.Development裡的名字一模一樣
			builder.Services.AddDbContext<ISpanShopDBContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

			//2號 依賴注入 (Dependency Injection)：別忘了在 Program.cs 中註冊這些 Service，否則執行時會報錯：

			builder.Services.AddScoped<PointService>();
			builder.Services.AddScoped<PaymentService>();
			builder.Services.AddScoped<CheckoutService>();


			//2號 尾吧


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

			app.Run();
		}
	}
}
