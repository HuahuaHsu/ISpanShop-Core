# ISpanShop 專案交接文件

## 一、專案概述

本專案為前後端分離的電商平台，後台已完成開發，目前正在進行前台功能開發。

---

## 二、專案架構

### 後端（ISpanShop.MVC）

```
ISpanShop.sln
├── ISpanShop.Common        # 工具、Enum、擴充方法
│   └── Enums/
│       ├── RoleEnum.cs         (Admin=1, Member=2)
│       └── MemberLevelEnum.cs  (Normal=1, Silver=2, Gold=3)
│
├── ISpanShop.Models        # EfModels、DTOs
│   ├── EfModels/
│   └── DTOs/
│       └── Auth/
│           ├── FrontLoginRequestDto.cs
│           ├── FrontLoginResponseDto.cs
│           └── FrontRegisterRequestDto.cs
│
├── ISpanShop.Repositories  # LINQ / CRUD
│   └── Members/
│       ├── IUserRepository.cs
│       └── UserRepository.cs
│
├── ISpanShop.Services      # 商業邏輯
│   ├── Admins/
│   ├── Auth/
│   │   ├── IFrontAuthService.cs
│   │   └── FrontAuthService.cs
│   └── Categories/
│
└── ISpanShop.MVC           # 後台 MVC + 前台 API Controller
    └── Controllers/
        └── Api/
            └── FrontAuthController.cs
```

### 前端（ISpanShop-Frontend）

```
src/
├── api/
│   ├── axios.ts        # axios instance + 攔截器（自動帶 Bearer token，401 導向 /login）
│   └── auth.ts         # loginApi(), registerApi()
├── components/
│   └── AppHeader.vue   # 右上角登入/登出按鈕
├── stores/
│   └── auth.ts         # Pinia：token、memberInfo、isLoggedIn、login()、logout()
├── types/
│   └── auth.ts         # LoginRequest、LoginResponse、RegisterRequest 型別
├── utils/
│   └── storage.ts      # getToken / setToken / removeToken
├── router/
│   ├── index.ts        # createRouter + beforeEach 路由守衛
│   └── routes.ts       # 路由清單
└── views/
    └── auth/
        ├── LoginView.vue
        └── RegisterView.vue
```

---

## 三、驗證架構

### 雙軌驗證（後台 Cookie + 前台 JWT，互不干擾）

| 區域 | 驗證方式 | Scheme 名稱 |
|---|---|---|
| 後台 MVC Admin | Cookie Authentication | `AdminCookieAuth` |
| 前台 API | JWT Bearer | `FrontendJwt` |

### Program.cs 設定重點
- 原本 `AdminCookieAuth` **不動**
- 新增 `.AddJwtBearer("FrontendJwt", ...)` 串接在後方
- CORS 已設定允許 `localhost:5173`

### appsettings.json JWT 區塊
```json
"Jwt": {
  "Key": "（32字元以上密鑰）",
  "Issuer": "ISpanShop",
  "Audience": "ISpanShopFrontend",
  "ExpireMinutes": 60
}
```

### JWT Claims 內容
| Claim | 來源欄位 |
|---|---|
| `ClaimTypes.NameIdentifier` | `User.Id` |
| `ClaimTypes.Email` | `User.Email` |
| `ClaimTypes.Name` | `MemberProfile.FullName` 或 `User.Account` |
| `"RoleId"` | `User.RoleId` |

---

## 四、命名規範

| 類型 | 規範 |
|---|---|
| ViewModel | 一律加 `Vm` 後綴（如 `MemberIndexVm`） |
| DTO | 放 `ISpanShop.Models/DTOs/{功能}/` |
| EfModel | 放 `ISpanShop.Models/EfModels/` |
| Repository | Interface 與實作放同一功能資料夾 |
| Service | Interface 與實作放同一功能資料夾 |
| MVC Models | `ISpanShop.MVC/Models/{功能}/` |
| MVC Views | `ISpanShop.MVC/Views/{功能}/` |
| API Controller | `ISpanShop.MVC/Controllers/Api/` |

---

## 五、DI 註冊清單（Program.cs）

```csharp
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IAdminRoleRepository, AdminRoleRepository>();
builder.Services.AddScoped<IFrontAuthService, FrontAuthService>();
```

---

## 六、前端路由現況

| 路徑 | 頁面 | 需登入 |
|---|---|---|
| `/login` | LoginView.vue | ❌ |
| `/register` | RegisterView.vue | ❌ |

> 其餘頁面（首頁、商品、購物車、會員中心等）尚未建立，新增時在 `routes.ts` 補上並設定 `meta: { requiresAuth: true/false }`。

---

## 七、前端 Pinia Auth Store 結構

```ts
state: {
  token: string | null
  isLoggedIn: boolean
  memberInfo: {
    memberId: number
    email: string
    memberName: string
  } | null
}

actions:
  login(form: LoginRequest): Promise<void>
  logout(): void
```

---

## 八、App.vue 目前狀態

套版（`DefaultLayout.vue`）尚未完成，`AppHeader` 暫時直接掛在 `App.vue`。
待套版組員完成後，將 `AppHeader` 移入 `DefaultLayout.vue`，`App.vue` 還原為單純 `<router-view />`。

```vue
<!-- App.vue 目前暫定 -->
<template>
  <el-container direction="vertical">
    <AppHeader />
    <el-main>
      <router-view />
    </el-main>
  </el-container>
</template>

<script setup lang="ts">
import AppHeader from './components/AppHeader.vue'
</script>
```

---

## 九、已完成功能清單

- [x] 後端 JWT 雙軌驗證設定（Program.cs）
- [x] DTO 定義（FrontLoginRequestDto / FrontLoginResponseDto / FrontRegisterRequestDto）
- [x] IUserRepository / UserRepository（分層規範）
- [x] FrontAuthService（JWT 簽發、登入、註冊）
- [x] FrontAuthController（POST /api/front/auth/login、/register）
- [x] Enum 消除 Magic Number（RoleEnum、MemberLevelEnum）
- [x] 前端 axios 設定（攔截器、CORS）
- [x] 前端 Pinia auth store
- [x] 路由守衛（beforeEach）
- [x] LoginView.vue（支援 Email 或帳號登入）
- [x] RegisterView.vue（帳號/密碼強度驗證、確認密碼欄位）
- [x] AppHeader.vue（登入/登出狀態切換）

---

## 十、下一步開發建議

前台驗證流程已完整建立，可依以下順序繼續開發：

1. **首頁 HomeView.vue** — 商品列表展示
2. **商品列表 ProductListView.vue** — 瀏覽、篩選、分頁
3. **商品詳情 ProductDetailView.vue** — 加入購物車
4. **購物車 CartView.vue** — 數量調整、刪除
5. **結帳 CheckoutView.vue** — 填寫收件資訊、送出訂單
6. **會員中心 MemberCenterView.vue** — 個人資料、訂單查詢

每個功能需同步在後端 `Controllers/Api/` 新增對應的 ApiController。
