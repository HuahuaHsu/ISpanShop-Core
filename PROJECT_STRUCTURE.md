# ISpanShop 專案架構與功能說明

更新日期：2026-05-03

## 1. 專案總覽

ISpanShop 是一套電商平台系統，採用後端 ASP.NET Core 8 多專案分層架構，搭配前端 Vue 3 SPA。後端同時提供後台 MVC 管理介面、前台 RESTful API、付款回呼端點與 SignalR 即時聊天；前端則負責消費者會員、商品瀏覽、購物車、結帳、賣家中心等互動頁面。

主要技術棧：

- 後端：ASP.NET Core 8 MVC / Web API
- ORM：Entity Framework Core，資料庫為 SQL Server
- 前端：Vue 3、Vite、TypeScript、Pinia、Vue Router、Element Plus
- 驗證：後台 Cookie Authentication，前台 JWT Bearer Authentication
- API 文件：Swagger / OpenAPI
- 即時通訊：SignalR
- 金流：NewebPay 服務，專案中也保留 ECPay helper

## 2. Solution 結構

```text
ISpanShop-Core1.0/
├─ ISpanShop.sln
├─ ISpanShop.MVC/             # ASP.NET Core Web 專案，MVC、API、後台 Area、SignalR、靜態資源
├─ ISpanShop.Models/          # EF Core 實體、DbContext、DTO、Migration、Seed 資料
├─ ISpanShop.Repositories/    # 資料存取層，封裝 EF Core 查詢與資料操作
├─ ISpanShop.Services/        # 商業邏輯層，串接 repository 並處理流程規則
├─ ISpanShop.Common/          # 共用 enum、helper、金流輔助、安全與 claims 工具
└─ ISpanShop-Frontend/        # Vue 3 + Vite 前台與賣家中心 SPA
```

後端專案依賴方向大致如下：

```text
ISpanShop.MVC
  ├─ ISpanShop.Services
  ├─ ISpanShop.Repositories
  ├─ ISpanShop.Models
  └─ ISpanShop.Common

ISpanShop.Services
  ├─ ISpanShop.Repositories
  ├─ ISpanShop.Models
  └─ ISpanShop.Common

ISpanShop.Repositories
  ├─ ISpanShop.Models
  └─ ISpanShop.Common

ISpanShop.Models
  └─ ISpanShop.Common
```

## 3. 後端框架與啟動流程

後端入口為 `ISpanShop.MVC/Program.cs`。

啟動時主要註冊內容：

- `AddControllersWithViews()`：同時支援 MVC View 與 API Controller。
- CORS policy `ISpanShopFrontendPolicy`：允許 `http://localhost:5173` 前端開發站台呼叫 API，並支援 credentials。
- `ISpanShopDBContext`：透過 `DefaultConnection` 連線 SQL Server，並啟用 SQL Server retry。
- Authentication：
  - `AdminCookieAuth`：後台 `/Admin` 使用 Cookie 登入。
  - `FrontendJwt`：前台 API 與 SignalR 使用 JWT。
- Swagger：開發環境啟用 `/swagger`，並支援 Bearer token 測試。
- SignalR：`ChatHub` 對應 `/chatHub`。
- 全域例外處理：`ExceptionHandlingMiddleware`。
- 資料初始化：啟動後執行 `DataSeeder`，補預設資料、商品審核資料、管理員帳號與部分開發期 schema 欄位。

預設開發 URL：

- 後端 HTTPS：`https://localhost:7125`
- 後端 HTTP：`http://localhost:5132`
- 前端 Vite：`http://localhost:5173`

## 4. 後端各專案職責

### 4.1 ISpanShop.MVC

此專案是後端 Web host，負責 HTTP request pipeline、Controller、View、API、SignalR hub、middleware 與靜態檔案。

重要目錄：

- `Controllers/`：一般 MVC controller、付款 controller、前台 API controller。
- `Controllers/Api/`：前台與賣家中心 API，例如會員、商品、分類、購物車、訂單、優惠券、賣場、客服、聊天。
- `Areas/Admin/`：後台管理區，包含 controller、view model 與 Razor views。
- `Hubs/`：SignalR 即時聊天 hub。
- `Middleware/`：全域例外處理。
- `Models/`：MVC 專用 ViewModel 或 request/response model。
- `Views/`：一般 MVC Razor views。
- `wwwroot/`：CSS、JS、圖片、uploads、Sneat 後台模板資源。

後台 Area 主要 view 模組：

- 管理員與權限：`Admin`、`Auth`、`LoginHistory`
- 會員管理：`Member`、`Points`
- 商品與分類：`Products`、`CategoryManage`、`CategoryAttributes`、`CategoryBinding`
- 訂單與物流：`Orders`、`OrderManagement`、`OrderTracking`、`OrderReviews`、`ReturnRequests`、`ShipmentWorkstation`
- 行銷與金流：`Coupons`、`PaymentManagement`
- 賣場與客服：`Stores`、`SupportTickets`
- 內容審核：`SensitiveWords`、`SensitiveWordCategories`

### 4.2 ISpanShop.Models

此專案是資料模型與資料傳輸物件集中處。

重要目錄：

- `EfModels/`：EF Core Power Tools 產生的 entity 與 `ISpanShopDBContext`。
- `DTOs/`：API 與 service 層使用的 DTO，依功能模組分類。
- `Migrations/`：EF Core migration。
- `Seeding/`：開發與初始化用種子資料。

主要資料表 / entity：

- 使用者與會員：`User`、`MemberProfile`、`MembershipLevel`、`Address`、`LoginHistory`、`BlacklistRecord`
- 權限：`Role`、`AdminLevel`、`Permission`
- 商品：`Product`、`ProductVariant`、`ProductImage`、`Brand`
- 分類與規格：`Category`、`CategoryAttribute`、`CategoryAttributeOption`、`CategoryAttributeMapping`
- 購物車與訂單：`Cart`、`CartItem`、`Order`、`OrderDetail`
- 評價與退貨：`OrderReview`、`ReviewImage`、`ReturnRequest`、`ReturnRequestItem`、`ReturnRequestImage`
- 行銷：`Promotion`、`PromotionItem`、`PromotionRule`、`Coupon`、`MemberCoupon`
- 金流與點數：`PaymentLog`、`PointHistory`
- 賣場與客服：`Store`、`SupportTicket`
- 即時訊息與審核：`ChatMessage`、`SensitiveWord`、`SensitiveWordCategory`

### 4.3 ISpanShop.Repositories

Repository 層封裝資料存取，避免 controller 或 service 直接散落複雜查詢。

主要模組：

- `Admins/`：管理員與角色資料。
- `Members/`：會員、地址、點數、登入紀錄、密碼重設 token。
- `Products/`、`Brands/`、`Categories/`：商品、品牌、分類與規格資料。
- `Inventories/`：庫存資料。
- `Orders/`：訂單與評價資料。
- `Promotions/`：促銷活動資料。
- `Stores/`：賣場資料。
- `Support/`：客服單資料。
- `ContentModeration/`：敏感字資料。
- `Communication/`：聊天資料。

### 4.4 ISpanShop.Services

Service 層負責商業流程、資料驗證、跨 repository 協作與 API 回傳前的資料整理。

主要模組：

- `Auth/`：前台登入、註冊、JWT、OAuth 相關流程。
- `Members/`：會員中心、地址、會員等級、登入紀錄。
- `Products/`、`Brands/`、`Categories/`：商品上下架、審核、品牌與分類規格管理。
- `Inventories/`：庫存查詢與調整。
- `Orders/`：購物車、前台訂單、後台訂單、訂單儀表板。
- `Payments/`：結帳、付款、NewebPay、點數。
- `Promotions/`、`Coupons/`：促銷與優惠券，並有 `CouponCleanupService` 背景清理服務。
- `Stores/`：賣場申請、賣場設定、前台賣場頁。
- `Support/`：客服單流程。
- `ContentModeration/`：敏感字管理。
- `Communication/`：Email 與聊天服務。

### 4.5 ISpanShop.Common

Common 放置跨層共用工具：

- `Enums/`：會員等級、訂單狀態、角色、賣場狀態等 enum。
- `Helpers/`：claims 讀取、安全工具、賣場狀態 helper。
- `EcpayHelper.cs`：ECPay 相關輔助。

## 5. 前端框架與結構

前端位於 `ISpanShop-Frontend/`，是 Vue 3 + Vite + TypeScript SPA。

主要套件：

- `vue`、`vue-router`：頁面與路由。
- `pinia`、`pinia-plugin-persistedstate`：前端狀態管理與持久化。
- `axios`：API 呼叫。
- `element-plus`、`@element-plus/icons-vue`：UI 元件。
- `@microsoft/signalr`：聊天功能。
- `apexcharts`、`vue3-apexcharts`：圖表。
- `@vueup/vue-quill`：富文字編輯器。

重要目錄：

- `src/api/`：依功能切分的 API client，例如 auth、product、cart、order、seller、store、support、promotion。
- `src/router/`：Vue Router 設定與全域守衛。
- `src/stores/`：Pinia store，例如 auth、cart、seller、chat、address。
- `src/views/`：前台、會員中心、賣家中心、認證、購物車與錯誤頁。
- `src/layouts/`：`DefaultLayout`、`MemberLayout`、`SellerLayout`、`BlankLayout`。
- `src/components/`：共用元件、商品元件、會員元件、訂單元件、聊天浮窗、賣家元件。
- `src/types/`：TypeScript 型別定義。
- `src/utils/`：格式化與 storage 工具。
- `src/styles/`：共用樣式。

開發環境設定：

- `.env.development` 設定 `VITE_API_BASE_URL=https://localhost:7125`。
- `vite.config.ts` 固定前端 port 為 `5173`。
- Vite proxy 將 `/api` 與 `/uploads` 導向 `https://localhost:7125`。

## 6. 前端主要路由與功能

公開前台：

- `/`：首頁。
- `/products`：商品列表。
- `/product/:id`：商品詳情。
- `/store/:id`：賣場頁。
- `/coupons`：優惠券頁。
- `/promotion/:id`：促銷活動詳情。

認證：

- `/login`：登入。
- `/register`：註冊。
- `/forgot-password`：忘記密碼。
- `/reset-password`：重設密碼。
- `/auth/callback`：OAuth callback。
- `/auth/oauth-merge`：OAuth 帳號合併。

會員中心：

- `/member`：會員中心首頁。
- `/member/profile`：個人資料。
- `/member/address`：地址管理。
- `/member/password`：密碼變更。
- `/member/level`：會員等級。
- `/member/orders`：訂單列表。
- `/member/orders/:id`：訂單詳情。
- `/member/orders/:id/refund`：退貨申請。
- `/member/orders/:id/review`：訂單評價。
- `/member/support`：客服單。
- `/member/wallet`：點數 / 錢包。
- `/member/coupons`：會員優惠券。
- `/member/mystore`：我的賣場。
- `/member/seller-apply`：申請成為賣家。

購物與付款：

- `/cart`：購物車。
- `/checkout`：結帳。
- `/payment/result`：付款結果。

賣家中心：

- `/seller`：賣家儀表板。
- `/seller/profile`：賣場設定。
- `/seller/products`：商品列表。
- `/seller/products/new`：新增商品。
- `/seller/products/:id/edit`：編輯商品。
- `/seller/products/:id/preview`：商品預覽。
- `/seller/orders`：賣家訂單。
- `/seller/orders/:id`：賣家訂單詳情。
- `/seller/returns`：退貨列表。
- `/seller/returns/:id`：退貨詳情。
- `/seller/promotions`：促銷管理。
- `/seller/coupons`：優惠券管理。
- `/seller/analytics/sales`：銷售報表。
- `/seller/analytics/traffic`：流量分析。

路由守衛重點：

- `requiresAuth`：需登入才能進入。
- `requiresSeller`：需具備賣家身分。
- `hideForAuth`：已登入者不應進入，例如登入與註冊頁。
- 停權帳號會被限制可造訪頁面，只保留基本瀏覽、會員中心、歷史訂單與客服申訴入口。

## 7. 主要功能模組

### 7.1 會員與認證

- 前台註冊、登入、JWT 發放。
- Cookie-based 後台登入。
- OAuth callback 與帳號合併流程。
- 忘記密碼與密碼重設 token。
- 會員資料、地址、會員等級、點數與停權狀態。

### 7.2 商品、分類、品牌與庫存

- 商品列表、詳情、圖片、影片、規格 variant。
- 商品上下架、刪除、審核、重新申請審核。
- 品牌管理。
- 分類樹、分類屬性、規格綁定與篩選設定。
- 庫存查詢、調整與安全庫存。

### 7.3 購物車、訂單、退貨與評價

- 會員購物車。
- 結帳與訂單建立。
- 前台訂單列表、詳情、付款狀態與退貨申請。
- 賣家訂單管理。
- 後台訂單管理、物流工作站、訂單追蹤。
- 訂單評價與評價圖片。

### 7.4 行銷與優惠

- 促銷活動與促銷商品。
- 促銷規則。
- 賣家優惠券與會員領券。
- 優惠券狀態清理背景服務。
- 前台優惠券與促銷頁展示。

### 7.5 金流與點數

- 結帳服務與付款流程。
- NewebPay 設定、付款導向與回傳處理。
- PaymentLog 記錄交易資訊。
- 會員點數歷程與點數餘額。

### 7.6 賣場與賣家中心

- 會員申請賣家。
- 賣場資料與狀態管理。
- 賣家商品、訂單、退貨、優惠券與促銷管理。
- 銷售報表與流量分析。
- 前台賣場頁。

### 7.7 客服、聊天與內容審核

- 客服單建立、查詢、處理與附件。
- SignalR 即時聊天。
- 敏感字與敏感字分類管理。
- 商品、評論或文字內容可透過敏感字資料做審核輔助。

## 8. API 與驗證設計

前台 API 多位於 `ISpanShop.MVC/Controllers/Api`，通常由 Vue 前端透過 axios 呼叫。前端 token 由 Pinia auth store 管理，API 使用 `FrontendJwt` 驗證。

後台管理使用 `Areas/Admin` 的 MVC controller 與 Razor view，驗證 scheme 為 `AdminCookieAuth`，登入頁位於 `/Admin/Auth/Login`。

Swagger 在 Development 環境啟用，可使用 Bearer token 測試前台 API。

SignalR 的 `/chatHub` 支援從 query string 讀取 `access_token` 或 `token`，用於前端建立聊天連線。

## 9. 資料庫與設定

預設連線字串位於 `ISpanShop.MVC/appsettings.json`：

```json
"DefaultConnection": "Server=.\\sql2025;Database=ISpanShopDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

其他重要設定：

- `Jwt`：JWT key、issuer、audience、過期分鐘數。
- `EmailSettings`：SMTP 設定。
- `NewebPaySettings`：MerchantID、HashKey、HashIV、付款回傳 URL。
- `Google`：OAuth client 設定。

注意：目前 `appsettings.json` 內含 JWT key、NewebPay HashKey/HashIV、Email 帳號等敏感設定。正式部署時應移至 User Secrets、環境變數或金鑰管理服務，避免提交到版本庫。

## 10. 開發執行方式

後端：

```powershell
dotnet run --project ISpanShop.MVC
```

或使用 Visual Studio 啟動 `ISpanShop.MVC` 的 `https` profile。

前端：

```powershell
cd ISpanShop-Frontend
npm install
npm run dev
```

前後端開發時，後端需跑在 `https://localhost:7125`，前端固定跑在 `http://localhost:5173`，以符合 CORS 與 Vite proxy 設定。

## 11. 維護建議

- 正式環境移除或保護開發用 `TestApiController`、`Development/TestController`。
- 將敏感設定移出 `appsettings.json`。
- 若資料庫 schema 已穩定，建議減少 `Program.cs` 內啟動時直接執行 ALTER TABLE 的補欄位邏輯，改以 migration 管理。
- 補上後端 service / repository 單元測試，以及前端關鍵流程測試。
- 統一 API response 格式與錯誤碼，方便前端集中處理。
- 為付款、訂單狀態轉換、優惠券併發使用等高風險流程補測試。

## 12. 使用者管理模組面試技術重點

本段聚焦使用者管理功能，包含前台會員登入、後台管理員登入、權限、驗證、登入紀錄、會員資料管理與管理員帳號管理。可作為面試時說明「我負責的模組」的整理稿。

### 12.1 模組範圍

負責範圍可整理為四條主線：

- 前台會員身分驗證：註冊、登入、JWT 簽發、Google OAuth、忘記密碼、重設密碼、會員資料修改。
- 後台管理員身分驗證：Cookie 登入、首次登入強制改密碼、RememberMe、登出、AccessDenied。
- 後台權限控管：管理員身分 `AdminLevel`、權限清單 `Permission`、身分權限對應 `AdminLevelPermissions`、自訂授權 filter。
- 稽核與管理：登入紀錄、會員黑名單、管理員建立、停用、重設密碼、角色 / 身分設定。

相關核心檔案：

- 前台登入 API：`ISpanShop.MVC/Controllers/Api/FrontAuthController.cs`
- 前台登入邏輯：`ISpanShop.Services/Auth/FrontAuthService.cs`
- 後台登入：`ISpanShop.MVC/Areas/Admin/Controllers/AuthController.cs`
- 管理員管理：`ISpanShop.MVC/Areas/Admin/Controllers/Admin/AdminController.cs`
- 管理員邏輯：`ISpanShop.Services/Admins/AdminService.cs`
- 權限 filter：`ISpanShop.MVC/Middleware/RequirePermissionAttribute.cs`、`RequireSuperAdminAttribute.cs`
- Claims helper：`ISpanShop.Common/Helpers/ClaimsPrincipalExtensions.cs`
- 密碼雜湊：`ISpanShop.Common/Helpers/SecurityHelper.cs`
- 登入紀錄：`ISpanShop.Repositories/Members/LoginHistoryRepository.cs`
- 會員管理：`ISpanShop.MVC/Areas/Admin/Controllers/Members/MemberController.cs`

### 12.2 架構設計說法

使用者管理模組沿用專案分層架構：

```text
Controller
  -> Service
    -> Repository
      -> SQL Server / EF Core DbContext
```

可以這樣說明：

- Controller 負責 HTTP request、ModelState、ViewModel / DTO 轉換、登入後寫入 Cookie 或回傳 JWT。
- Service 負責商業規則，例如密碼強度、首次登入流程、黑名單判斷、是否可停用自己、是否保留至少一位超級管理員。
- Repository 負責資料存取。會員與登入紀錄多使用 EF Core；管理員查詢與身分權限管理使用 ADO.NET 搭配參數化 SQL。
- Common 放跨模組共用邏輯，例如 BCrypt 密碼雜湊與 Claims 權限判斷。

這個切法的重點是讓驗證流程、權限規則與資料存取分離，方便維護與測試，也避免 Controller 直接塞大量 SQL 或商業判斷。

### 12.3 前台會員登入與 JWT

前台登入流程：

```text
Vue LoginView
  -> POST /api/front/auth/login
  -> FrontAuthController.Login
  -> FrontAuthService.LoginAsync
  -> UserRepository.GetByEmailOrAccountAsync
  -> BCrypt 驗證密碼
  -> 寫入 LoginHistory
  -> 回傳 JWT 與會員摘要資料
```

技術重點：

- 前台 API 使用 `FrontendJwt` authentication scheme。
- JWT claims 包含：
  - `NameIdentifier`：UserId
  - `Email`
  - `Name`
  - `RoleId`
  - `IsBlacklisted`
  - 若使用者有賣場，額外加入 `StoreId`
- JWT 使用 HMAC SHA256 簽章，issuer、audience、key、expire minutes 由 `appsettings.json` 的 `Jwt` section 設定。
- 前台路由守衛會根據登入狀態、賣家身分與停權狀態控制可進入頁面。
- API controller 使用 `[Authorize(AuthenticationSchemes = "FrontendJwt")]` 保護需要登入的端點。

面試可強調：

- 前台選 JWT 是因為 Vue SPA 與後端 API 分離，token-based auth 更適合前後端分離與行動端擴充。
- JWT 只放必要身分資訊，不放敏感資料，例如密碼、個資明細。
- 停權狀態同時在登入 response 與 JWT claim 中提供，前端可即時限制路由，後端查詢商品 / 促銷時也會排除黑名單賣家。

### 12.4 後台管理員登入與 Cookie Authentication

後台登入流程：

```text
GET /Admin/Auth/Login
POST /Admin/Auth/Login
  -> AdminService.VerifyLogin
  -> AdminRepository.GetAdminByAccount
  -> BCrypt.Verify
  -> 寫入 LoginHistory
  -> 判斷 IsFirstLogin
  -> 寫入 AdminCookieAuth cookie
```

後台與前台不同，使用 `AdminCookieAuth`：

- 後台是 MVC Razor View，適合使用 Cookie Authentication。
- 未登入時導向 `/Admin/Auth/Login`。
- 權限不足時導向 `/Admin/Auth/AccessDenied`。
- Cookie 名稱為 `ISpanShop.Admin`。
- RememberMe 勾選時 Cookie 有效期為 30 天；未勾選時為 30 分鐘。

首次登入設計：

- 新增管理員或重設管理員密碼後，`IsFirstLogin = true`。
- 首次登入成功後，只建立短效暫時 Cookie，期限 15 分鐘。
- 暫時 claims 包含 UserId、Account、`IsFirstLogin=true`。
- 使用者必須進入 `ChangePassword` 修改密碼。
- 修改成功後，系統登出舊 Cookie，要求重新登入以取得完整權限 claims。

面試可強調：

- 後台用 Cookie，前台用 JWT，是依照使用場景分開設計，不混用同一套驗證機制。
- 首次登入強制改密碼可以避免管理員臨時密碼長期有效。
- RememberMe 影響 cookie persistence 與過期時間，兼顧便利性與安全性。

### 12.5 密碼安全與重設密碼

密碼安全由 `SecurityHelper` 統一處理：

- `ToBCrypt(password)`：使用 BCrypt 產生雜湊，BCrypt 會自帶 salt。
- `Verify(password, hashedPassword)`：登入時比對明文密碼與資料庫雜湊。
- 不儲存明文密碼。

前台忘記密碼流程：

```text
POST /api/front/auth/forgot-password
  -> AccountService.ForgotPasswordAsync
  -> 查詢使用者
  -> 刪除舊 token
  -> 建立 30 分鐘有效的一次性 token
  -> 發送重設密碼 email

POST /api/front/auth/reset-password
  -> 驗證 token 是否存在、未使用、未過期
  -> BCrypt 雜湊新密碼
  -> 更新 Users.Password
  -> 刪除該使用者所有 reset token
```

安全細節：

- Forgot password 即使 Email 不存在，也回傳類似成功訊息，避免帳號探測。
- 重設 token 用 GUID `N` 格式產生，設定 30 分鐘有效期。
- 新密碼以 BCrypt 雜湊後才寫入資料庫。
- OAuth 純帳號若尚未設定密碼，變更密碼流程可略過舊密碼檢查。

### 12.6 Google OAuth 與帳號合併

前台支援 Google OAuth：

- `/api/front/auth/oauth/google`：使用 authorization code 向 Google 換 token，讀取 id token 取得 `sub`、`email`、`name`。
- 若 `Provider=Google` 且 `ProviderId` 已存在，直接登入並回 JWT。
- 若 email 已存在但尚未綁定 Google，回傳 `MergeRequired`，要求使用者驗證原帳號密碼後合併。
- 若完全新帳號，建立新 User 與 MemberProfile。
- 已登入會員可綁定或解除 Google OAuth。

面試可強調：

- OAuth 登入不是只看 email 直接登入，而是用 provider + providerId 作為主要外部身分識別。
- email 已存在時進入帳號合併流程，避免第三方登入誤綁既有帳號。
- 解除綁定前要求帳號已有本地密碼，避免使用者解除後無法登入。

### 12.7 後台權限模型

資料模型：

```text
Users
  -> RoleId
  -> AdminLevelId

AdminLevels
  -> AdminLevelPermissions
    -> Permissions
```

概念區分：

- `Role`：大身分，例如 Member、Admin、SuperAdmin。
- `AdminLevel`：後台管理員等級 / 職務，例如超級管理員、客服、商品管理等。
- `Permission`：具體功能權限，例如 `member_manage`、`product_manage`、`cashflow_manage`、`ticket_manage`、`sensitive_manage`、`report_view`。
- `AdminLevelPermissions`：一個管理員身分可擁有哪些功能權限。

登入後 claims 設計：

- `NameIdentifier`：管理員 UserId。
- `Name`：帳號。
- `Role`：角色名稱。
- `AdminLevelId`：管理員身分 ID。
- `LevelName`：身分名稱。
- `Permission`：每個 PermissionKey 各自加入一個 claim。

授權方式：

- 一般後台 controller 可使用 `[Authorize]` 要求登入。
- 需要特定功能權限時使用 `[RequirePermission("permission_key")]`。
- 只有超級管理員可進入時使用 `[RequireSuperAdmin]` 或檢查 `AdminLevelId == 1`。
- `ClaimsPrincipalExtensions.IsSuperAdmin()` 判斷 `AdminLevelId == 1`。
- `ClaimsPrincipalExtensions.HasPermission()` 先判斷超級管理員，否則檢查 claims 中是否有指定 `Permission`。

面試可強調：

- 權限不是寫死在角色名稱，而是透過 `AdminLevel` 與 `Permission` 做資料庫化設定。
- 登入時把權限 key 放入 claims，後續 request 不需要每次查資料庫即可授權。
- 超級管理員採 shortcut：`AdminLevelId=1` 直接擁有所有權限。

### 12.8 管理員管理功能

管理員管理主要由 `AdminController`、`AdminService`、`AdminRepository` 處理。

功能包含：

- 管理員列表：關鍵字、狀態、AdminLevel 篩選、排序、分頁。
- 新增管理員：
  - 自動產生帳號，例如 `ADM001`。
  - 產生預設 email。
  - 密碼 BCrypt hash 後寫入。
  - 新帳號 `IsFirstLogin = true`。
  - 不允許直接建立超級管理員身分。
- 編輯管理員：
  - 修改 AdminLevel。
  - 修改黑名單狀態。
  - 不允許將一般管理員升級為超級管理員。
- 停用管理員：
  - 使用 `IsBlacklisted = true` 軟停用。
  - 不允許停用自己。
  - 若目標是超級管理員，必須確保至少保留一位啟用中的超級管理員。
- 重設管理員密碼：
  - 臨時密碼至少 8 碼。
  - BCrypt hash 後更新。
  - 將 `IsFirstLogin` 設回 true，要求下次登入改密碼。
- 管理員身分管理：
  - 新增 / 修改 / 刪除 AdminLevel。
  - 設定該身分擁有的 PermissionIds。
  - 更新權限時使用 transaction，先刪除舊對應再新增新對應。
  - 系統預設身分不可修改或刪除。
  - 若仍有啟用管理員綁定該身分，禁止刪除。

面試可強調：

- 管理員管理有防呆規則，例如不可停用自己、不可刪最後一位超級管理員、不可直接升級超級管理員。
- 身分與權限設定使用 transaction，避免 AdminLevel 更新成功但權限對應失敗造成資料不一致。
- 管理員列表支援搜尋、排序、分頁，是後台管理常見需求。

### 12.9 會員後台管理

會員管理主要由 `MemberController`、`MemberService`、`MemberRepository` 處理。

功能包含：

- 會員列表：
  - 關鍵字搜尋：帳號、Email、姓名、電話。
  - 狀態篩選：正常 / 停權。
  - 身分篩選：買家 / 賣家。
  - 會員等級篩選：依累積消費金額與會員等級門檻判斷。
  - 排序與分頁。
- 會員編輯：
  - Email、姓名、電話、性別、生日。
  - 頭像 URL。
  - 黑名單狀態 `IsBlacklisted`。
  - 是否為賣家 `IsSeller`。
  - 預設地址的縣市、區域、街道。
- 編輯頁支援完整頁面與 AJAX partial view，方便後台 drawer 操作。

會員等級計算：

- 會員等級不是只讀 profile 上的 LevelName，而是根據 `TotalSpending` 對照 `MembershipLevels.MinSpending`。
- 取符合門檻且金額最高的等級。

面試可強調：

- 後台會員管理排除 Admin / SuperAdmin，只處理一般會員與賣家。
- 黑名單狀態會影響前台可用功能與部分資料查詢，例如商品 / 促銷排除黑名單賣家。
- 會員等級用消費金額動態推算，可以減少等級名稱不同步問題。

### 12.10 登入紀錄與稽核

登入紀錄資料表為 `LoginHistories`，主要欄位：

- `UserId`：成功登入時寫入使用者 ID，帳號不存在時可為 null。
- `AttemptedAccount`：嘗試登入的帳號。
- `LoginTime`：登入嘗試時間。
- `IPAddress`：來源 IP。
- `IsSuccess`：是否登入成功。

寫入時機：

- 前台會員登入成功或失敗都會寫入。
- 後台管理員登入成功、密碼錯誤、帳號不存在、黑名單拒絕登入都會寫入。

查詢功能：

- 後台登入紀錄頁由 `LoginHistoryController` 提供。
- 僅超級管理員可看，使用 `[RequireSuperAdmin]`。
- 支援關鍵字搜尋帳號或 IP。
- 支援成功 / 失敗篩選。
- 支援排序與分頁。
- Repository 使用 `AsNoTracking()` 提升查詢效能。

面試可強調：

- 登入紀錄不是只記成功登入，而是失敗也記，才有稽核與資安分析價值。
- 帳號不存在時仍記錄 `AttemptedAccount` 與 IP，可以用來觀察暴力嘗試或撞庫行為。
- 登入紀錄查詢屬於敏感後台功能，只開放超級管理員。

### 12.11 前後台驗證機制比較

| 項目 | 前台會員 | 後台管理員 |
| --- | --- | --- |
| 使用者介面 | Vue SPA | ASP.NET Core MVC Razor |
| 驗證機制 | JWT Bearer | Cookie Authentication |
| Scheme | `FrontendJwt` | `AdminCookieAuth` |
| Token / Cookie 內容 | UserId、Email、RoleId、IsBlacklisted、StoreId | UserId、Role、AdminLevelId、LevelName、Permission |
| 權限控制 | API `[Authorize(AuthenticationSchemes = "FrontendJwt")]`、前端 router guard | `[Authorize]`、`[RequirePermission]`、`[RequireSuperAdmin]` |
| 過期設定 | `Jwt:ExpireMinutes` | 30 分鐘或 RememberMe 30 天 |
| 主要用途 | 前台 API、會員中心、賣家中心、SignalR | 後台管理頁與 MVC action |

面試可說：

> 我將前後台驗證分開設計：前台是 Vue SPA，所以用 JWT 保護 API；後台是 MVC 管理系統，所以用 Cookie Authentication。兩者在 `Program.cs` 同時註冊不同 scheme，Controller 透過指定 scheme 或自訂授權 filter 控制存取。

### 12.12 可用於面試的專案亮點說法

可用 1 分鐘版本：

> 我負責使用者管理模組，包含前台會員 JWT 登入、後台管理員 Cookie 登入、BCrypt 密碼雜湊、Google OAuth、忘記密碼 token、登入紀錄、會員黑名單與後台權限控管。前台和後台採不同 authentication scheme，前台 API 使用 JWT，後台 MVC 使用 Cookie。後台登入後會把 AdminLevelId 與 PermissionKey 放進 claims，再透過自訂的 RequirePermissionAttribute 做授權判斷；超級管理員 AdminLevelId=1 擁有所有權限。登入成功或失敗都會寫入 LoginHistory，方便稽核與追蹤異常登入。

可用 3 分鐘版本：

> 這個模組我分成驗證、授權與稽核三塊。驗證方面，前台會員登入使用 JWT，登入時用 BCrypt 驗證密碼，簽發包含 UserId、RoleId、IsBlacklisted 與 StoreId 的 token；後台管理員使用 Cookie Authentication，支援 RememberMe，並且對新建或重設密碼的管理員設計首次登入強制改密碼流程。授權方面，我把後台權限資料庫化，使用 AdminLevel、Permission、AdminLevelPermissions 三張表管理身分與功能權限，登入後把 PermissionKey 寫入 claims，Controller 再用 RequirePermissionAttribute 檢查，不需要每次 request 都查資料庫。稽核方面，不論前後台登入成功或失敗都寫 LoginHistory，包含嘗試帳號、IP、時間與成功狀態，後台提供超級管理員查詢、篩選、排序與分頁。

### 12.13 面試可能被問到的問題與回答方向

Q：為什麼前台用 JWT、後台用 Cookie？

A：前台是 Vue SPA，和 API 分離，JWT 比較適合前後端分離架構，也方便未來行動端使用；後台是 MVC Razor 頁面，Cookie Authentication 對 server-rendered 管理介面較自然，能直接處理登入頁導向與 AccessDenied。

Q：權限是怎麼做的？

A：管理員有 `AdminLevelId`，每個 AdminLevel 對應多個 Permission。登入時查出 PermissionKey 並放進 claims。需要權限的 controller 或 action 加上 `[RequirePermission("member_manage")]`，filter 會先檢查是否登入，再檢查是否超級管理員，否則比對 claims 中是否有該 PermissionKey。

Q：如何保護密碼？

A：使用 BCrypt，不存明文密碼。註冊、建立管理員、重設密碼時都 hash 後存入；登入時用 BCrypt.Verify 比對。BCrypt 會自帶 salt，也能抵抗彩虹表攻擊。

Q：忘記密碼怎麼避免被猜帳號？

A：使用者輸入不存在的 Email 時，也回傳類似「若已註冊則已寄送」的訊息，避免攻擊者用回應差異確認帳號是否存在。

Q：登入紀錄記哪些資料？為什麼失敗也要記？

A：記 UserId、AttemptedAccount、LoginTime、IP、IsSuccess。失敗紀錄能用來追蹤密碼猜測、撞庫、異常 IP，也可作為客服或資安稽核依據。

Q：如何避免管理員誤操作造成系統無人可管？

A：停用管理員時不允許停用自己；若目標是超級管理員，會檢查啟用中的超級管理員數量，至少保留一位。也不允許一般流程直接建立或升級成超級管理員。

Q：如果權限被更新，已登入管理員會立刻生效嗎？

A：目前權限是在登入時寫入 claims，所以已登入的 Cookie 會保留當時的權限；若要即時生效，可在更新權限後強制登出相關管理員、縮短 Cookie 期限，或改成每次授權查詢資料庫 / 加版本號檢查。

### 12.14 可再強化的地方

- 將 JWT key、金流 key、SMTP 帳密移到 User Secrets、環境變數或正式的 secrets manager。
- 管理員權限異動後，加入強制重新登入或 security stamp 機制，避免舊 claims 繼續有效。
- LoginHistory 可加上 UserAgent、失敗原因、地理位置或裝置資訊，提升稽核價值。
- 登入失敗可加入 rate limiting、帳號鎖定或 CAPTCHA，降低暴力破解風險。
- Reset password token 可改為儲存雜湊 token，避免資料庫外洩時 token 可直接使用。
- 前台 JWT 可搭配 refresh token 與 token rotation，改善長效 token 風險。
- 管理員 Repository 目前使用 ADO.NET，後續可評估與 EF Core 統一，或保留 ADO.NET 但建立更明確的 query helper 降低重複 SQL。
