【前台 JWT 驗證實作】

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 1：後端 — 模型定義
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 1. 建立 FrontLoginRequestDto
         位置：ISpanShop.Models 專案 -> DTOs -> Auth 資料夾 (需新增) -> FrontLoginRequestDto.cs
         說明：接收前端傳來的登入帳密，欄位對照 Member EfModel。
         內容：Email、Password

  [ ] 2. 建立 FrontLoginResponseDto
         位置：ISpanShop.Models 專案 -> DTOs -> Auth 資料夾 -> FrontLoginResponseDto.cs
         說明：登入成功後回傳給前端的資料。
         內容：Token、MemberId、Email、MemberName (依 Member EfModel 欄位命名)

  [ ] 3. 建立 FrontRegisterRequestDto
         位置：ISpanShop.Models 專案 -> DTOs -> Auth 資料夾 -> FrontRegisterRequestDto.cs
         說明：前台會員註冊所需欄位，對照 Member EfModel。

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 2：後端 — Program.cs 設定
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 4. 安裝 NuGet 套件
         位置：ISpanShop.MVC 專案
         動作：安裝 Microsoft.AspNetCore.Authentication.JwtBearer

  [ ] 5. 新增 JWT 設定到 appsettings.json
         位置：ISpanShop.MVC 專案 -> appsettings.json
         動作：在原本設定下方新增
         "Jwt": {
           "Key": "你的密鑰(32字元以上)",
           "Issuer": "ISpanShop",
           "Audience": "ISpanShopFrontend",
           "ExpireMinutes": 60
         }

  [ ] 6. 在 Program.cs 新增 JWT 驗證方案
         位置：ISpanShop.MVC 專案 -> Program.cs
         動作：在原本 AdminCookieAuth 的 .AddCookie(...) 後方串接，不動原有設定
         .AddJwtBearer("FrontendJwt", options => { ... })

  [ ] 7. 設定 CORS
         位置：ISpanShop.MVC 專案 -> Program.cs
         動作：新增 AddCors，允許 Vue dev server (localhost:5173) 存取

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 3：後端 — Service 層
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 8. 建立介面 IFrontAuthService
         位置：ISpanShop.Services 專案 -> Interfaces 資料夾 -> IFrontAuthService.cs
         說明：定義前台登入與註冊的方法合約。
         內容：
         Task<FrontLoginResponseDto?> LoginAsync(FrontLoginRequestDto request);
         Task<bool> RegisterAsync(FrontRegisterRequestDto request);

  [ ] 9. 實作 FrontAuthService
         位置：ISpanShop.Services 專案 -> (根目錄) -> FrontAuthService.cs
         說明：注入 MemberRepository，實作以下流程：
         登入：查詢 Member -> 比對密碼 hash -> 簽發 JWT (寫入 MemberId、Email 等 Claims)
         註冊：驗證 Email 不重複 -> 建立新 Member -> 儲存

  [ ] 10. 註冊服務到 DI 容器
          位置：ISpanShop.MVC 專案 -> Program.cs
          動作：在 var app = builder.Build(); 之前加入
          builder.Services.AddScoped<IFrontAuthService, FrontAuthService>();

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 4：後端 — API Controller
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 11. 建立 FrontAuthController
          位置：ISpanShop.MVC 專案 -> Controllers -> Api 資料夾 -> FrontAuthController.cs
          說明：前台登入/註冊 API 進入點，不加 [Authorize]（登入前不需驗證）。
          實作：
          POST /api/front/auth/login    -> 呼叫 LoginAsync，回傳 FrontLoginResponseDto
          POST /api/front/auth/register -> 呼叫 RegisterAsync，回傳成功/失敗訊息

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 5：前端 — 基礎設施
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 12. 實作 storage.ts
          位置：src/utils/storage.ts
          說明：封裝 localStorage 操作，提供 getToken、setToken、removeToken 方法。

  [ ] 13. 定義 Auth 相關型別
          位置：src/types/auth.ts  (需新增)
          說明：對應後端 DTO，定義 LoginRequest、LoginResponse、RegisterRequest 介面。
          欄位命名需與後端 DTO 一致。

  [ ] 14. 實作 auth.ts (API 層)
          位置：src/api/auth.ts
          說明：定義 loginApi()、registerApi() 函式，透過 axios 呼叫後端 API。

  [ ] 15. 設定 axios.ts
          位置：src/api/axios.ts
          說明：建立 axios instance 並設定：
          baseURL 指向後端
          Request 攔截器：自動帶入 Authorization: Bearer <token>
          Response 攔截器：收到 401 → 清除 token → 導向 /login

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 6：前端 — Pinia Store
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 16. 實作 auth store
          位置：src/stores/auth.ts
          說明：
          state：token、memberInfo (MemberId、Email、MemberName)、isLoggedIn
          action login()：呼叫 loginApi -> 存 token -> 解析 JWT payload -> 更新 state
          action logout()：清除 token + 重置 state
          持久化：安裝 pinia-plugin-persistedstate 或手動寫入 localStorage

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 7：前端 — 路由守衛
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 17. 設定路由 meta 標記
          位置：src/router/routes.ts
          說明：需要登入的頁面加上 meta: { requiresAuth: true }

  [ ] 18. 設定全域路由守衛
          位置：src/router/index.ts
          說明：新增 beforeEach：
          requiresAuth: true 且未登入 -> 導向 /login
          已登入進入 /login -> 導向首頁

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 8：前端 — 登入/註冊頁面
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 19. 建立 LoginView.vue
          位置：src/views/auth/LoginView.vue
          說明：使用 Element Plus el-form，呼叫 authStore.login()，成功後導向首頁。

  [ ] 20. 建立 RegisterView.vue
          位置：src/views/auth/RegisterView.vue
          說明：使用 Element Plus el-form，呼叫 registerApi()，成功後導向登入頁。