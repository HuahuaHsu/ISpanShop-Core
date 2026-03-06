using ISpanShop.Models.EfModels;
using ISpanShop.Repositories;
using ISpanShop.Repositories.Interfaces;
using ISpanShop.Services;
using ISpanShop.Services.Interfaces;
using ISpanShop.WebAPI.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace ISpanShop.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── DbContext（與後台 MVC 共用同一個資料庫）──
            builder.Services.AddDbContext<ISpanShopDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ── Repositories ──
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

            // ── Services ──
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();

            // ── Controllers ──
            builder.Services.AddControllers();

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

            // ── 開發環境：啟用 Swagger UI ──
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ISpanShop 前台 API v1");
                    c.RoutePrefix = string.Empty; // 讓 Swagger UI 在根路徑 / 顯示
                });
            }

            // ── 全域例外處理（最優先，包在最外層）──
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseHttpsRedirection();

            app.UseCors("FrontendPolicy");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
