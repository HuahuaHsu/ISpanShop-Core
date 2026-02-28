using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;
// 1. 新增你需要的 using
using ISpanShop.Repositories;
using ISpanShop.Services;
using ISpanShop.WebAPI.Hubs;

namespace ISpanShop.WebAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// 資料庫連線設定
			builder.Services.AddDbContext<ISpanShopDBContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

			builder.Services.AddControllers();
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			// ==========================================
			// 2. 註冊 SignalR 與你的 Services / Repositories
			// ==========================================
			builder.Services.AddSignalR();
			builder.Services.AddScoped<IChatRepository, ChatRepository>();
			builder.Services.AddScoped<ISensitiveWordsRepository, SensitiveWordsRepository>();
			builder.Services.AddScoped<IChatService, ChatService>();

			// ?? 非常重要：加入 CORS 政策
			// 因為你的前端 (MVC) 呼叫 WebAPI 時會跨網域，SignalR 需要允許傳遞憑證
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowAll", policy =>
				{
					policy.AllowAnyMethod()
						  .AllowAnyHeader()
						  .SetIsOriginAllowed(origin => true) // 允許任何來源 (開發階段方便測試)
						  .AllowCredentials();                // SignalR 必須允許憑證
				});
			});

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			// ==========================================
			// 3. 啟用 CORS (必須放在 UseAuthorization 之前)
			// ==========================================
			app.UseCors("AllowAll");

			app.UseAuthorization();

			app.MapControllers();

			// ==========================================
			// 4. 設定 SignalR Hub 的路由端點
			// ==========================================
			app.MapHub<ChatHub>("/chatHub");

			app.Run();
		}
	}
}