using ISpanShop.Models.EfModels;
using ISpanShop.Models.Seeding;
using ISpanShop.MVC.Middleware;
using ISpanShop.MVC.Hubs;

// Repository namespaces
using ISpanShop.Repositories.Admins;
using ISpanShop.Repositories.Members;
using ISpanShop.Repositories.Members.Implementations;
using ISpanShop.Repositories.Products;
using ISpanShop.Repositories.Categories;
using ISpanShop.Repositories.Orders;
using ISpanShop.Repositories.Inventories;
using ISpanShop.Repositories.ContentModeration;
using ISpanShop.Repositories.Support;
using ISpanShop.Repositories.Stores;
using ISpanShop.Repositories.Promotions;
using ISpanShop.Repositories.Brands;
using ISpanShop.Repositories.Communication;

// Service namespaces
using ISpanShop.Services.Admins;
using ISpanShop.Services.Members;
using ISpanShop.Services.Products;
using ISpanShop.Services.Categories;
using ISpanShop.Services.Orders;
using ISpanShop.Services.Inventories;
using ISpanShop.Services.ContentModeration;
using ISpanShop.Services.Support;
using ISpanShop.Services.Payments;
using ISpanShop.Services.Stores;
using ISpanShop.Services.Promotions;
using ISpanShop.Services.Brands;
using ISpanShop.Services.Coupons;
using ISpanShop.Services.Auth;
using ISpanShop.Services.Communication;
using ISpanShop.Services;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace ISpanShop.MVC
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // ── 1. CORS 服務註冊 ──
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ISpanShopFrontendPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // ── 2. 資料庫連線 ──
            builder.Services.AddDbContext<ISpanShopDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

            // ── 3. 身份驗證 ──
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            builder.Services.AddAuthentication("AdminCookieAuth")
                .AddCookie("AdminCookieAuth", options =>
                {
                    options.Cookie.Name = "ISpanShop.Admin";
                    options.LoginPath = "/Admin/Auth/Login";
                    options.AccessDeniedPath = "/Admin/Auth/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                })
                .AddJwtBearer("FrontendJwt", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
                        ClockSkew = TimeSpan.Zero // 減少時間誤差導致的 401
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            if (string.IsNullOrEmpty(accessToken))
                            {
                                accessToken = context.Request.Query["token"];
                            }

                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/chatHub") || path.StartsWithSegments("/api/chat")))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogWarning($"Authentication failed: {context.Exception.Message}");
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            // 避免 API 請求被導向登入頁面，而是回傳 401
                            if (context.Request.Path.StartsWithSegments("/api"))
                            {
                                context.HandleResponse();
                                context.Response.StatusCode = 401;
                                context.Response.ContentType = "application/json";
                                return context.Response.WriteAsync("{\"message\":\"未授權，請先登入\"}");
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            // ── 4. DI 服務註冊 ──
            builder.Services.AddScoped<IFrontAuthService, FrontAuthService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IMemberRepository, MemberRepository>();
            builder.Services.AddScoped<IMemberService, MemberService>();
            builder.Services.AddScoped<IPointRepository, PointRepository>();
            builder.Services.AddScoped<PointService>();
            builder.Services.AddScoped<IAdminRepository, AdminRepository>();
            builder.Services.AddScoped<IAdminRoleRepository, AdminRoleRepository>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<ILoginHistoryRepository, LoginHistoryRepository>();
            builder.Services.AddScoped<ILoginHistoryService, LoginHistoryService>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICategoryAttributeRepository, CategoryAttributeRepository>();
            builder.Services.AddScoped<CategoryAttributeService>();
            builder.Services.AddScoped<ICategoryManageRepository, CategoryManageRepository>();
            builder.Services.AddScoped<CategoryManageService>();
            builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();
            builder.Services.AddScoped<IBrandRepository, BrandRepository>();
            builder.Services.AddScoped<BrandService>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IFrontOrderService, FrontOrderService>();
            builder.Services.AddScoped<IOrderDashboardService, OrderDashboardService>();
            builder.Services.AddScoped<IOrderReviewRepository, OrderReviewRepository>();
            builder.Services.AddScoped<IOrderReviewService, OrderReviewService>();
            builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
            builder.Services.AddScoped<PromotionService>();
            builder.Services.AddScoped<ICouponService, CouponService>();
            builder.Services.AddHostedService<CouponCleanupService>();
            builder.Services.AddScoped<PaymentService>();
            builder.Services.AddScoped<CheckoutService>();
            builder.Services.AddScoped<NewebPayService>();
            builder.Services.AddScoped<ISensitiveWordRepository, SensitiveWordRepository>();
            builder.Services.AddScoped<ISensitiveWordService, SensitiveWordService>();
            builder.Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
            builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
            builder.Services.AddScoped<IStoreRepository, StoreRepository>();
            builder.Services.AddScoped<IStoreService, StoreService>();
            builder.Services.AddScoped<IFrontStoreService, FrontStoreService>();
			// ── 4. DI 服務註冊 ──

			// 核心身分與會員
			builder.Services.AddScoped<IFrontAuthService, FrontAuthService>();
			builder.Services.AddScoped<IAccountService, AccountService>();
			builder.Services.AddScoped<IEmailService, EmailService>();
			builder.Services.AddScoped<IUserRepository, UserRepository>();
			builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
			builder.Services.AddScoped<IMemberRepository, MemberRepository>();
			builder.Services.AddScoped<IMemberService, MemberService>();
			builder.Services.AddScoped<IPointRepository, PointRepository>();
			builder.Services.AddScoped<PointService>();

			// 後台管理
			builder.Services.AddScoped<IAdminRepository, AdminRepository>();
			builder.Services.AddScoped<IAdminRoleRepository, AdminRoleRepository>();
			builder.Services.AddScoped<IAdminService, AdminService>();
			builder.Services.AddScoped<ILoginHistoryRepository, LoginHistoryRepository>();
			builder.Services.AddScoped<ILoginHistoryService, LoginHistoryService>();

			// 商品與分類
			builder.Services.AddScoped<IProductRepository, ProductRepository>();
			builder.Services.AddScoped<IProductService, ProductService>();
			builder.Services.AddScoped<ICategoryAttributeRepository, CategoryAttributeRepository>();
			builder.Services.AddScoped<CategoryAttributeService>();
			builder.Services.AddScoped<ICategoryManageRepository, CategoryManageRepository>();
			builder.Services.AddScoped<CategoryManageService>();
			builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
			builder.Services.AddScoped<IInventoryService, InventoryService>();
			builder.Services.AddScoped<IBrandRepository, BrandRepository>();
			builder.Services.AddScoped<BrandService>();

			// 訂單、評論與行銷
			builder.Services.AddScoped<IOrderRepository, OrderRepository>();
			builder.Services.AddScoped<IOrderService, OrderService>();
			builder.Services.AddScoped<IFrontOrderService, FrontOrderService>();
			builder.Services.AddScoped<IOrderDashboardService, OrderDashboardService>();
			builder.Services.AddScoped<IOrderReviewRepository, OrderReviewRepository>();
			builder.Services.AddScoped<IOrderReviewService, OrderReviewService>();
			builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
			builder.Services.AddScoped<PromotionService>();
			builder.Services.AddScoped<ICouponService, CouponService>();
			builder.Services.AddHostedService<CouponCleanupService>();

			// 支付與系統
			builder.Services.AddScoped<PaymentService>();
			builder.Services.AddScoped<CheckoutService>();
			builder.Services.AddScoped<NewebPayService>();
			builder.Services.AddScoped<ISensitiveWordRepository, SensitiveWordRepository>();
			builder.Services.AddScoped<ISensitiveWordService, SensitiveWordService>();
			builder.Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
			builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
			builder.Services.AddScoped<IStoreRepository, StoreRepository>();
			builder.Services.AddScoped<IStoreService, StoreService>();
			builder.Services.AddScoped<IFrontStoreService, FrontStoreService>();

            // 聊聊系統
            builder.Services.AddScoped<IChatRepository, ChatRepository>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IBotService, MockBotService>(); // 機器人服務
            builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
            builder.Services.AddSignalR();

            // ── 5. Swagger ──
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ISpanShop API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                        new string[] {}
                    }
                });
            });

            var app = builder.Build();

            // ── 6. Pipeline ──
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("ISpanShopFrontendPolicy");
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ISpanShop API v1"));
            }

            // ── 7. 路由 ──
            app.MapHub<ChatHub>("/chatHub");
            app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute(name: "default", pattern: "{controller=Orders}/{action=Dashboard}/{id?}", defaults: new { area = "Admin" });
            app.MapControllerRoute(name: "home", pattern: "Home/{action=Index}/{id?}");
            app.MapControllers();

            // ── 8. 初始化 ──
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ISpanShopDBContext>();
                try
                {
                    await DataSeeder.SeedAsync(context);
                    await EnsureDatabaseSchemaAsync(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "初始化錯誤");
                }
            }
            app.Run();
        }

        private static async Task EnsureDatabaseSchemaAsync(ISpanShopDBContext context)
        {
            var sqlCommands = new[]
            {
                "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'IsDeleted') ALTER TABLE Products ADD IsDeleted BIT NOT NULL DEFAULT 0",
                "IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ReviewStatus') ALTER TABLE Products ADD ReviewStatus INT NOT NULL DEFAULT 0"
            };
            foreach (var sql in sqlCommands) 
            { 
                try { await context.Database.ExecuteSqlRawAsync(sql); } catch { /* 忽略重複執行錯誤 */ } 
            }
        }
    }
}
