# ISpanShop 認證概覽（Admin Cookie 與 Frontend JWT）

此文件為初學者快速上手指南，說明專案中兩種認證機制（後台 Cookie、前台 JWT）的比較、程式碼位置與建置範例。

## 一、架構總覽
- 後台（Admin）使用 Cookie 認證（針對 Razor MVC / Areas/Admin）。
  - Scheme 名稱：AdminCookieAuth
  - 相關位置：ISpanShop.MVC/Program.cs、ISpanShop.MVC/Areas/Admin/Controllers/AuthController.cs、Areas/Admin 下之 Controller。
- 前台（Frontend API）使用 JWT Bearer（針對 RESTful API Controllers）。
  - Scheme 名稱：FrontendJwt
  - 相關位置：ISpanShop.MVC/Program.cs、ISpanShop.MVC/Controllers/Api/FrontAuthController.cs、其他 Controllers/Api/*。

## 二、比較（簡短）
- Cookie (Admin)
  - 優點：與瀏覽器表單/視圖整合容易（自動帶 Cookie），簡單管理 Claims、Role。
  - 缺點：跨域或跨子域 API 使用不便；較不適合 SPA 或行動應用直接呼叫。
  - 適用場景：後台管理介面、Razor Pages、需要伺服端 Session 風格體驗。

- JWT (Frontend)
  - 優點：無狀態、跨域使用方便、適合 SPA/行動 App；API 可使用 Authorization Header 傳遞。
  - 缺點：Token 撤銷較麻煩（需黑名單或短過期 + refresh token），在前端須妥善儲存（避免 XSS）。
  - 適用場景：前台 SPA、移動裝置、第三方 API 授權。

## 三、程式碼/建置範例（重點片段）

- Program.cs（註冊 Authentication）

```csharp
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
    });

app.UseAuthentication();
app.UseAuthorization();
```

- 後台發放 Cookie（AuthController）

```csharp
// 建立 Claims 與 ClaimsIdentity
var claimsIdentity = new ClaimsIdentity(claims, "AdminCookieAuth");
await HttpContext.SignInAsync("AdminCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);
```

- 前台使用 JWT（Controller 標記）

```csharp
[Authorize(AuthenticationSchemes = "FrontendJwt")]
public class OrdersController : ControllerBase { ... }
```

- FrontAuthController -> Login 應呼叫 IFrontAuthService.LoginAsync，該方法負責驗證帳密並回傳 JWT（包含簽發、過期、Claim）。

## 四、實務建議與 Checklist
- 新增 Admin Controller：放在 Areas/Admin，設定 [Area("Admin")]，並在需要時加 [Authorize] 或 [Authorize(Roles = "...")]。
- 新增 API Controller：若需要登入保護，請加 [Authorize(AuthenticationSchemes = "FrontendJwt")]。
- 避免混淆：不要在 Admin 區域使用 FrontendJwt；不要把 API 直接靠 Cookie 作為唯一認證（在跨域情況會有問題）。
- 搜尋關鍵字：
  - AdminCookieAuth
  - FrontendJwt
  - AuthenticationSchemes
  - [Area("Admin")]
  - [Authorize(AuthenticationSchemes = "FrontendJwt")]

## 五、排錯提示
- 若 API 回 401：檢查 controller 是否有 Authorize 與正確的 AuthenticationSchemes，檢查 Program.cs 的 Jwt 設定（Issuer/Audience/Key）與 appsettings.json 的 Jwt 區段。
- 若 Admin 登入無效：檢查 Cookie 名稱、LoginPath、SignInAsync 使用的 scheme 是否一致，確認 UseAuthentication 在 UseAuthorization 之前已呼叫。

## 六、尚未標記 FrontendJwt 的 API controllers（建議清單與原因）
下列為 Controllers/Api 下目前「未標註 FrontendJwt」的檔案與其公開方法/路由，並提供是否需要保護的建議與理由。檔名以紅色標示，方便提醒（Markdown 內使用 HTML span）。

- <span style="color:red">BrandApiController.cs</span>
  - GET  /api/brands  → GetBrands()
  - 建議：不需保護（AllowAnonymous 已標註）。理由：公開前台品牌列表，供商品頁/搜尋使用。

- <span style="color:red">CategoryApiController.cs</span>
  - GET  /api/categories  → GetCategories()
  - GET  /api/categories/{id}/children  → GetChildren(int id)
  - 建議：不需保護（AllowAnonymous 已標註）。理由：公開分類資料給前台瀏覽/篩選。

- <span style="color:red">CategoriesApiController.cs</span>
  - GET  /api/categories/{categoryId}/attributes  → GetAttributes(int categoryId)
  - 建議：需保護。理由：此端點主要供賣家建立/編輯商品時使用，回傳規格屬性，應限制為 authenticated seller 或 admin；若要公開給前台顯示，建議拆分成公開版與管理版。

- <span style="color:red">SellerProductsApiController.cs</span>
  - GET  /api/seller/products  → GetProducts(...)
  - GET  /api/seller/products/{id}  → GetProduct(int id)
  - POST /api/seller/products  → CreateProduct([FromForm]...)
  - PUT  /api/seller/products/{id}  → UpdateProduct(int id,...)
  - DELETE /api/seller/products/{id} → DeleteProduct(int id)
  - 建議：需保護（強烈）。理由：包含建立/修改/刪除等寫入操作，應限制為該賣家或管理員。

- <span style="color:red">SellerVariantsApiController.cs</span>
  - GET  /api/seller/products/{productId}/variants  → GetVariants(int productId)
  - POST /api/seller/products/{productId}/variants  → AddVariant(...)
  - PUT  /api/seller/products/{productId}/variants/{variantId} → UpdateVariant(...)
  - DELETE /api/seller/products/{productId}/variants/{variantId} → DeleteVariant(...)
  - 建議：需保護（強烈）。理由：寫入/修改/刪除賣家資料，應限制為該商品的擁有者。

- <span style="color:red">SellerInventoryApiController.cs</span>
  - GET  /api/seller/inventory  → GetList(...)
  - GET  /api/seller/inventory/summary  → GetSummary(...)
  - GET  /api/seller/inventory/{variantId}  → GetDetail(int variantId)
  - PUT  /api/seller/inventory/{variantId}/stock  → UpdateStock(...)
  - PUT  /api/seller/inventory/{variantId}/safety-stock → UpdateSafetyStock(...)
  - PUT  /api/seller/inventory/{variantId} → UpdateInventory(...)
  - 建議：需保護（強烈）。理由：涉及庫存調整（寫入），必須驗證呼叫者為相應賣家或管理員。

- <span style="color:red">PromotionApiController.cs</span>
  - GET /api/promotions/active → GetActivePromotions(string? type, int limit)
  - 建議：不需保護（AllowAnonymous 已標註）。理由：活動資訊為公開前台內容。

- <span style="color:red">ProductsApiController.cs</span>
  - GET /api/products → GetProducts(...)
  - GET /api/products/{id} → GetByIdAsync(int id)
  - GET /api/products/{id}/related → GetRelatedAsync(int id, int limit)
  - 建議：不需保護（AllowAnonymous 已標註）。理由：商品列表/詳情為公開內容。

- <span style="color:red">FrontProfileController.cs</span>
  - GET  /api/front/profile/{id} → GetProfile(int id)
  - PUT  /api/front/profile/{id} → UpdateProfile(int id, UpdateMemberProfileDto)
  - 建議：需保護（強烈）。理由：涉及個人資料讀取與更新，應限本人或有管理權限者存取。

### 保護範例程式碼（類別層）
在 controller 類別上加入：
```csharp
[Authorize(AuthenticationSchemes = "FrontendJwt")]
[ApiController]
[Route("api/seller/products")]
public class SellerProductsApiController : ControllerBase { ... }
```

### 身分/擁有者驗證範例（方法內）
```csharp
// 取得 JWT 中的 user id（在簽發 token 時需包含 ClaimTypes.NameIdentifier）
var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
if (!int.TryParse(userIdClaim, out var userId))
    return Unauthorized();

// 假設有方法可以檢查該 userId 是否為該店家擁有者
if (!IsOwnerOfStore(userId, storeId))
    return Forbid();
```

### 建議流程
1. 先在需要保護的 controller 類別上加上 [Authorize(AuthenticationSchemes = "FrontendJwt")]。
2. 在寫入或敏感操作方法內，使用 Claim 驗證呼叫者身份並檢查資源所有權（storeId、memberId 等）。
3. 如需不同角色（seller / admin / buyer），可建立自訂 Policy 並在 Startup/Program 中註冊：
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SellerOnly", policy =>
        policy.RequireClaim("Role", "Seller"));
});
```

---
檔案已更新於專案根目錄：AuthGuide.md。後續要我直接把建議的 [Authorize] 標註加入到那些 controller 嗎？