using ISpanShop.MVC.Hubs;
using ISpanShop.Repositories.Communication;
using ISpanShop.Services.Communication;

// ... (other using statements)

namespace ISpanShop.MVC
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllersWithViews();

			// ── 1. CORS 服務註冊 (合併為單一政策) ──
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("ISpanShopFrontendPolicy", policy =>
				{
					// 開發環境：允許 Vite (localhost:5173) 
					policy.WithOrigins("http://localhost:5173")
						  .AllowAnyHeader()
						  .AllowAnyMethod()
						  .AllowCredentials(); // 支援未來可能需要的 Cookie 傳遞
				});
			});

			// ── 2. 資料庫連線 ──
            // ... (rest of DB connection code)
			builder.Services.AddDbContext<ISpanShopDBContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
				sqlServerOptionsAction: sqlOptions =>
				{
					sqlOptions.EnableRetryOnFailure(
						maxRetryCount: 5,
						maxRetryDelay: TimeSpan.FromSeconds(30),
						errorNumbersToAdd: null);
				}));

			// ── 3. 身份驗證 (Cookie 後台 + JWT 前台) ──
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
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
					};

					// 支援 SignalR 從 Query String 取得 Token
					options.Events = new JwtBearerEvents
					{
						OnMessageReceived = context =>
						{
							var accessToken = context.Request.Query["access_token"];
							var path = context.HttpContext.Request.Path;
							if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
							{
								context.Token = accessToken;
							}
							return Task.CompletedTask;
						}
					};
				});

			// ── 4. DI 服務註冊 ──

			// 核心身分與會員
            // ... (rest of auth registrations)
			builder.Services.AddScoped<IFrontAuthService, FrontAuthService>();
			builder.Services.AddScoped<IUserRepository, UserRepository>();
			builder.Services.AddScoped<IMemberRepository, MemberRepository>();
			builder.Services.AddScoped<IMemberService, MemberService>();
			builder.Services.AddScoped<IPointRepository, PointRepository>();
			builder.Services.AddScoped<PointService>();

			// 後台管理
            // ... (rest of admin registrations)
			builder.Services.AddScoped<IAdminRepository, AdminRepository>();
			builder.Services.AddScoped<IAdminRoleRepository, AdminRoleRepository>();
			builder.Services.AddScoped<IAdminService, AdminService>();
			builder.Services.AddScoped<ILoginHistoryRepository, LoginHistoryRepository>();
			builder.Services.AddScoped<ILoginHistoryService, LoginHistoryService>();

			// 商品與分類
            // ... (rest of product registrations)
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
            // ... (rest of order registrations)
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
            // ... (rest of payment registrations)
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
			builder.Services.AddSignalR(); // 啟用 SignalR

			// ── 5. Swagger / OpenAPI ──
            // ... (rest of swagger config)
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = "ISpanShop API",
					Version = "v1",
					Description = "ISpanShop 電商平台 RESTful API (後台 Cookie + 前台 JWT)"
				});

				// 讓 Swagger 支援輸入 JWT Token 進行測試
				c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Name = "Authorization",
					Type = SecuritySchemeType.ApiKey,
					Scheme = "Bearer",
					BearerFormat = "JWT",
					In = ParameterLocation.Header,
					Description = "請輸入 JWT Token，格式為: Bearer {YourToken}"
				});
				c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
						},
						new string[] {}
					}
				});
			});

			var app = builder.Build();

			// ── 6. HTTP Request Pipeline ──
            // ... (rest of app pipeline)
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			// CORS 必須在 Routing 之後，Authentication 之前
			app.UseCors("ISpanShopFrontendPolicy");

			// 全域例外處理
			app.UseMiddleware<ExceptionHandlingMiddleware>();

			// 驗證與授權 (順序不可對調)
			app.UseAuthentication();
			app.UseAuthorization();

			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI(c =>
				{
					c.SwaggerEndpoint("/swagger/v1/swagger.json", "ISpanShop API v1");
				});
			}

			// ── 7. 路由設定 ──
			app.MapHub<ChatHub>("/chatHub"); // 對應 ChatHub 路由

			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Orders}/{action=Dashboard}/{id?}",
				defaults: new { area = "Admin" });

			app.MapControllerRoute(
				name: "home",
				pattern: "Home/{action=Index}/{id?}");

			app.MapControllers();

			// ── 8. 資料初始化與結構補強 ──
            // ... (rest of seeding code)
			using (var scope = app.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				var context = services.GetRequiredService<ISpanShopDBContext>();

				try
				{
					await DataSeeder.SeedAsync(context);
					await DataSeeder.PatchMissingReviewDataAsync(context);
					await DataSeeder.EnsurePendingProductsAsync(context);
					await DataSeeder.EnsureAdminUserAsync(context);

					// 執行資料庫欄位補強 (針對開發階段的 Schema 調整)
					await EnsureDatabaseSchemaAsync(context);
				}
				catch (Exception ex)
				{
					var logger = services.GetRequiredService<ILogger<Program>>();
					logger.LogError(ex, "資料初始化過程中發生錯誤");
				}
			}

			app.Run();
		}

		/// <summary>
		/// 確保開發階段所需的資料庫欄位存在
		/// </summary>
		private static async Task EnsureDatabaseSchemaAsync(ISpanShopDBContext context)
        {
            // ... (rest of schema code)
			var sqlCommands = new[]
			{
				"IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'IsDeleted') ALTER TABLE Products ADD IsDeleted BIT NOT NULL DEFAULT 0",
				"IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ReviewStatus') ALTER TABLE Products ADD ReviewStatus INT NOT NULL DEFAULT 0",
				"IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ReviewedBy') ALTER TABLE Products ADD ReviewedBy NVARCHAR(100) NULL",
				"IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ReviewDate') ALTER TABLE Products ADD ReviewDate DATETIME NULL",
				"IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Categories' AND COLUMN_NAME = 'NameEn') ALTER TABLE Categories ADD NameEn NVARCHAR(200) NULL",
				"IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CategoryAttributeMappings' AND COLUMN_NAME = 'Sort') ALTER TABLE CategoryAttributeMappings ADD Sort INT NOT NULL DEFAULT 0",
				"IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CategoryAttributes' AND COLUMN_NAME = 'AllowCustomInput') ALTER TABLE CategoryAttributes ADD AllowCustomInput BIT NOT NULL DEFAULT 0",
				"UPDATE Products SET Name = SUBSTRING(Name, 7, LEN(Name) - 6) WHERE LEFT(Name, 6) = N'[待審核] '"
			};

			foreach (var sql in sqlCommands)
			{
				try { await context.Database.ExecuteSqlRawAsync(sql); } catch { /* 忽略重複執行錯誤 */ }
			}
		}
	}
}