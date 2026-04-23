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

【變更密碼 實作】

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 1：後端 — 模型定義
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 1. 建立 ChangePasswordDto
         位置：ISpanShop.Models 專案 -> DTOs -> Auth 資料夾 -> ChangePasswordDto.cs
         說明：已登入使用者變更密碼時，接收舊密碼與新密碼。
         內容：UserId、OldPassword、NewPassword

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 2：後端 — Repository 層
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 2. 確認 UserRepository 已有 GetByIdAsync
         位置：ISpanShop.Repositories 專案 -> UserRepository.cs
         說明：若尚未實作，補上依 UserId 查詢使用者的方法，供 Service 層驗證密碼使用。

  [ ] 3. UserRepository 新增 UpdatePasswordHashAsync
         位置：ISpanShop.Repositories 專案 -> UserRepository.cs
         說明：依 UserId 更新資料庫中的密碼 Hash。
         方法簽章：Task UpdatePasswordHashAsync(int userId, string newHash)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 3：後端 — Service 層
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 4. 建立 IAccountService 介面
         位置：ISpanShop.Services 專案 -> Interfaces -> IAccountService.cs
         說明：定義帳號相關業務邏輯的抽象介面。
         方法：ChangePasswordAsync

  [ ] 5. 建立 AccountService
         位置：ISpanShop.Services 專案 -> AccountService.cs
         說明：實作 IAccountService，包含以下方法：

         ChangePasswordAsync(ChangePasswordDto dto)
           - 依 UserId 取出使用者
           - 驗證舊密碼 Hash 是否正確（使用 PasswordHasher）
           - 確認新密碼不與舊密碼相同
           - 呼叫 UpdatePasswordHashAsync 更新密碼
           - 回傳 (bool Success, string Message)

  [ ] 6. 在 Program.cs 註冊服務
         位置：WebAPI 或 MVC 專案 -> Program.cs
         說明：
           builder.Services.AddScoped<IAccountService, AccountService>();

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 4：後端 — API Controller
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 7. 新增變更密碼 API
         位置：FrontMemberController.cs（或新建 FrontAccountController.cs）
         路由：PUT /api/front/member/password
         說明：需加上 [Authorize]，從 JWT Token 取出 UserId，
               呼叫 AccountService.ChangePasswordAsync，回傳成功或失敗訊息。

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 5：前端
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 8. 實作 PasswordView.vue
         位置：src/views/member/PasswordView.vue（已存在，補上實作）
         說明：表單包含舊密碼、新密碼、確認新密碼三個欄位，
               串接 PUT /api/front/member/password，
               成功後清除本地 Token 並導向 /login。



【忘記密碼 實作】（變更密碼完成後再開始）

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 1：後端 — 模型定義
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 1. 建立 ForgotPasswordDto
         位置：ISpanShop.Models 專案 -> DTOs -> Auth 資料夾 -> ForgotPasswordDto.cs
         說明：忘記密碼時，接收使用者輸入的 Email 以觸發寄信流程。
         內容：Email

  [ ] 2. 建立 ResetPasswordDto
         位置：ISpanShop.Models 專案 -> DTOs -> Auth 資料夾 -> ResetPasswordDto.cs
         說明：使用者點擊信件連結後，接收 Token 與新密碼完成密碼重設。
         內容：Token、NewPassword

  [ ] 3. 建立 PasswordResetToken EfModel
         位置：ISpanShop.Models 專案 -> EfModels -> PasswordResetToken.cs
         說明：對應資料庫 PasswordResetTokens 資料表。
         內容：Id、UserId (FK)、Token、ExpiresAt、IsUsed、CreatedAt

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 2：後端 — 資料庫
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 4. 新增 PasswordResetTokens 資料表
         位置：DbContext -> 新增 DbSet<PasswordResetToken>
         說明：設定 UserId FK 對應 Users 資料表。
         執行：Add-Migration AddPasswordResetTokens -> Update-Database

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 3：後端 — Repository 層
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 5. 建立 IPasswordResetTokenRepository 介面
         位置：ISpanShop.Repositories 專案 -> Interfaces -> IPasswordResetTokenRepository.cs
         說明：定義 Token 相關資料操作的抽象介面。
         方法：CreateTokenAsync、GetValidTokenAsync、MarkTokenUsedAsync

  [ ] 6. 建立 PasswordResetTokenRepository
         位置：ISpanShop.Repositories 專案 -> PasswordResetTokenRepository.cs
         說明：實作 IPasswordResetTokenRepository，直接操作 DbContext。
         方法說明：
           - CreateTokenAsync(int userId, string token, DateTime expiresAt)：建立新 Token 紀錄
           - GetValidTokenAsync(string token)：查詢未過期且 IsUsed = false 的 Token
           - MarkTokenUsedAsync(string token)：將 Token 的 IsUsed 設為 true

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 4：後端 — Service 層
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 7. 建立 IEmailService 介面
         位置：ISpanShop.Common 專案 -> Helpers -> IEmailService.cs
         說明：定義寄送 Email 的抽象介面。
         方法：Task SendAsync(string toEmail, string subject, string htmlBody)

  [ ] 8. 建立 EmailService
         位置：ISpanShop.Common 專案 -> Helpers -> EmailService.cs
         說明：使用 MailKit 實作寄信，SMTP 設定寫在 appsettings.json，
               透過建構子注入 IConfiguration 讀取。

  [ ] 9. IAccountService 介面補上忘記密碼相關方法
         位置：ISpanShop.Services 專案 -> Interfaces -> IAccountService.cs
         新增方法：ForgotPasswordAsync、ResetPasswordAsync

  [ ] 10. AccountService 補上忘記密碼相關方法
          位置：ISpanShop.Services 專案 -> AccountService.cs

          ForgotPasswordAsync(string email)
            - 查詢 Email 是否存在（不存在時仍回傳成功，避免洩漏帳號資訊）
            - 產生 GUID Token，有效期限 30 分鐘
            - 呼叫 CreateTokenAsync 存入資料庫
            - 組合重設連結：{前端網址}/reset-password?token={Token}
            - 呼叫 IEmailService 寄出重設密碼信

          ResetPasswordAsync(ResetPasswordDto dto)
            - 呼叫 GetValidTokenAsync 驗證 Token 是否有效
            - Token 無效或已過期則回傳失敗訊息
            - 依 Token 找到對應 UserId，更新新密碼 Hash
            - 呼叫 MarkTokenUsedAsync 將 Token 標記為已使用
            - 回傳 (bool Success, string Message)

  [ ] 11. 在 Program.cs 補上新增服務註冊
          位置：WebAPI 或 MVC 專案 -> Program.cs
          說明：
            builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
            builder.Services.AddSingleton<IEmailService, EmailService>();

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 5：後端 — API Controller
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 12. 新增忘記密碼 API
          位置：FrontAuthController.cs
          路由：POST /api/front/auth/forgot-password
          說明：不需 [Authorize]，接收 Email，呼叫 AccountService.ForgotPasswordAsync。

  [ ] 13. 新增驗證 Token API
          位置：FrontAuthController.cs
          路由：GET /api/front/auth/verify-reset-token?token=xxx
          說明：不需 [Authorize]，前端重設密碼頁面載入時呼叫，
                Token 無效則前端顯示「連結已失效」。

  [ ] 14. 新增重設密碼 API
          位置：FrontAuthController.cs
          路由：POST /api/front/auth/reset-password
          說明：不需 [Authorize]，接收 Token + NewPassword，
                呼叫 AccountService.ResetPasswordAsync，成功後導引前端回登入頁。

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 6：前端
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 15. LoginView.vue 新增「忘記密碼？」連結
          位置：src/views/auth/LoginView.vue
          說明：在密碼欄位下方加上連結，點擊後導向 /forgot-password。

  [ ] 16. 新增 ForgotPasswordView.vue
          位置：src/views/auth/ForgotPasswordView.vue
          說明：單一 Email 輸入欄位，送出後顯示「請查收信箱，連結 30 分鐘內有效」提示，
                使用 BlankLayout 版型。

  [ ] 17. 新增 ResetPasswordView.vue
          位置：src/views/auth/ResetPasswordView.vue
          說明：頁面載入時從網址取出 Token，呼叫 verify-reset-token 驗證，
                Token 無效則顯示「連結已失效，請重新申請」；
                Token 有效則顯示新密碼 + 確認新密碼表單，
                送出後串接 POST /api/front/auth/reset-password，成功後導向 /login。
                使用 BlankLayout 版型。

  [ ] 18. 新增路由設定
          位置：src/router/index.js
          說明：新增以下兩條路由，皆使用 BlankLayout，不需 requiresAuth：
            { path: '/forgot-password', component: ForgotPasswordView }
            { path: '/reset-password', component: ResetPasswordView }

【開通賣場】

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 1：後端 — 模型與介面定義
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 1. 建立 StoreApplyRequestDto
         位置：ISpanShop.Models 專案 -> DTOs -> Stores 資料夾 -> StoreApplyRequestDto.cs
         說明：接收前端傳來的賣場申請資料。
         內容：StoreName (必填)、Description (選填)、LogoUrl (選填)

  [ ] 2. 更新 IFrontStoreService 介面
         位置：ISpanShop.Services 專案 -> Interfaces -> IFrontStoreService.cs
         新增方法：
         Task<bool> ApplyStoreAsync(int userId, StoreApplyRequestDto dto);
         Task<string> GetStoreStatusAsync(int userId); // 回傳狀態：NotApplied, Pending, Approved, Rejected

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 2：後端 — Service 與 API 實作
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 3. 實作 FrontStoreService 申請邏輯
         位置：ISpanShop.Services 專案 -> Stores -> FrontStoreService.cs
         說明：
         1. 檢查該會員是否已有 Store 紀錄 (避免重複申請)。
         2. 新增 Store：IsVerified 設為 null (待審核), StoreStatus 設為 2 (休假中)。
         3. 暫不更新 MemberProfile.IsSeller，待後台審核通過後再更新。

  [ ] 4. 建立 FrontStoreController (Api)
         位置：ISpanShop.MVC 專案 -> Controllers -> Api -> FrontStoreController.cs
         實作：
         POST /api/front/store/apply  -> [Authorize] 接收 DTO 並呼叫 ApplyStoreAsync。
         GET /api/front/store/status -> [Authorize] 回傳該會員目前的申請進度。

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 3：前端 — 介面與導航邏輯
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 5. 定義 Store 相關型別與 API 呼叫
         位置：src/types/store.ts, src/api/store.ts
         說明：建立對應後端 DTO 的型別，並實作 axios 呼叫。

  [ ] 6. 建立 SellerApplyView.vue (申請頁面)
         位置：src/views/member/SellerApplyView.vue
         說明：使用 Element Plus 製作申請表單。包含賣場名稱、描述與提交按鈕。

【賣家申請與後台審核流程】

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 1：後端 — 模型與 DTO (ISpanShop.Models)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 1. 建立 StoreReviewDto
         位置：ISpanShop.Models 專案 -> DTOs -> Stores 資料夾 -> StoreReviewDto.cs
         內容：StoreId (int)、IsPassed (bool)、ReviewNote (string, 駁回原因)

  [ ] 2. 建立 StoreDetailDto (管理後台用)
         位置：ISpanShop.Models 專案 -> DTOs -> Stores 資料夾 -> StoreDetailDto.cs
         說明：顯示申請內容供管理員審核。
         內容：StoreId、MemberName、StoreName、Description、ApplyTime、IsVerified

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 2：後端 — 管理後台實作 (ISpanShop.MVC / Services)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 3. 更新 IStoreService 與 StoreService (後台版)
         位置：ISpanShop.Services 專案 -> Stores 資料夾
         新增方法：
         - GetPendingStoresAsync()：取得所有 IsVerified 為 null 的申請案。
         - ReviewStoreAsync(StoreReviewDto dto)：執行審核。
           邏輯：
           - 若 IsPassed 為 true：
             - 更新 Store.IsVerified = true。
             - 更新 MemberProfile.IsSeller = true。
           - 若 IsPassed 為 false：
             - 更新 Store.IsVerified = false。
             - 更新 Store.ReviewNote = dto.ReviewNote (需在 EfModel 新增此欄位)。

  [ ] 4. 實作 Admin StoreController
         位置：ISpanShop.MVC 專案 -> Areas -> Admin -> Controllers -> StoreController.cs
         動作：
         - Index：列表顯示所有 Pending 狀態的賣場。
         - Review (POST)：接收審核結果並呼叫 Service。

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 3：後端 — 前台 API 調整
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 5. 調整 FrontStoreService.ApplyStoreAsync
         位置：ISpanShop.Services 專案 -> Stores -> FrontStoreService.cs
         邏輯優化：
         - 如果該會員已存在 Store 紀錄且狀態為「Rejected (IsVerified=false)」：
           - 允許「更新」現有紀錄（StoreName, Description, Logo）並將 IsVerified 重設為 null。
           - 這樣就不會重複產生多筆 Store 紀錄，達成「重新申請」功能。

  [ ] 6. 調整 FrontStoreController.GetStoreStatus
         位置：ISpanShop.MVC 專案 -> Controllers -> Api -> FrontStoreController.cs
         說明：回傳資料需包含 ReviewNote，讓使用者知道為何被駁回。

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 4：前端 — 狀態跳轉與重新申請 (Vue)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [ ] 7. 強化 MyStoreView.vue
         位置：src/views/member/MyStoreView.vue
         邏輯處理：
         - `status === 'NotApplied'`：顯示「成為賣家」大按鈕。
         - `status === 'Pending'`：顯示「申請審核中，請耐心等候」提示。
         - `status === 'Rejected'`：
           - 顯示「申請未通過：[原因]」。
           - 提供「重新編輯申請」按鈕，點擊後跳轉至申請頁面並帶入原有資料。
         - `status === 'Approved'`：渲染賣家儀表板 (Dashboard)。

  [ ] 8. 優化 SellerApplyView.vue
         位置：src/views/member/SellerApplyView.vue
         功能：
         - 進入頁面時先檢查是否為「重新申請」。
         - 若為重新申請，先取得上次申請的內容填入表單。
         - 提交按鈕根據狀態判斷是呼叫「新增」還是「更新」API。

         

【會員等級功能 實作】 (完成於 2026-04-23)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 1：後端 — API 與 邏輯
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [x] 1. 在 MemberApiController 新增 GetLevelInfo API
         路由：GET /api/member/level-info
         功能：
         - 實時從資料庫讀取 MemberProfile.TotalSpending (確保數據真實性)。
         - 查詢 MembershipLevels 資料表，獲取所有等級規則 (Id, Name, MinSpending, DiscountRate)。
         - 回傳 JSON：包含總消費金額、目前等級名稱及完整的等級規則列表。

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▌Phase 2：前端 — API 與 元件串接
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [x] 2. 封裝 API 請求
         位置：src/api/member.ts
         函式：export const getLevelInfo = () => axios.get('/api/member/level-info');

  [x] 3. 實作 LevelView.vue 核心邏輯
         位置：src/views/member/LevelView.vue
         功能：
         - 初始化載入：onMounted 時呼叫 getLevelInfo 並啟動 v-loading 狀態。
         - 動態判定等級：使用 computed 根據 realTotalSpending 遍歷規則，自動匹配目前等級。
         - 進度計算：精確計算目前金額距離下一階門檻的百分比，並動態提示所需金額。

  [x] 4. UI/UX 視覺優化
         - 顏色設定：一般會員 (品牌橘 #EE4D2D)、銀卡 (灰藍)、金卡 (琥珀金)。
         - 閃爍修復：移除 el-progress 的 striped-flow 效果，改為穩定色塊。
         - 表格邏輯：一般會員 (折扣率=1) 在專屬權益欄位顯示「—」，隱藏冗餘折扣資訊。
         - 動態區間：自動計算最近 12 個月的統計日期 (startDate ～ endDate)。

