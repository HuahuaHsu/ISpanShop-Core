//以下為已完成的MVC後台功能專案資料夾//

**ISpanShop.sln (解決方案)**
├── ISpanShop.Models       # 類別庫 (專放**EfModels**、**DTOs**)
├── ISpanShop.Repositories   # 類別庫 (專注於寫 LINQ、CRUD 資料庫操作)
├── ISpanShop.Services       # 類別庫 (核心商業邏輯，例如結帳計算、檢查庫存)
├── ISpanShop.Common         # 類別庫 (放共用工具：檔案上傳服務、常數、擴充方法)
├── ISpanShop.MVC             ASP.NET Core MVC (後台管理系統)，內部原生**Models**可寫**ViewModel**

**注意** : 關於Model，請依照位置創建檔案⇒

ISpanShop.Models ⇒ **EfModels**、**DTOs**
ISpanShop.MVC ⇒ **Models ⇒ ViewModel**

**ISpanShop 全專案開發流程規範表**

| **專案名稱** | **資料夾路徑** | **推薦命名規則 (以產品功能為例)** | **職責與開發重點** |
| --- | --- | --- | --- |
| **ISpanShop.Common** | **Enums/**
**Helpers/**
**Extensions/** | `StatusEnum.cs`
`SecurityHelper.cs`
`DateExtensions.cs` | **基礎建設**：定義全專案共用的狀態、工具與擴充方法。不依賴其他專案。 |
| **ISpanShop.Models** | **EfModels/**
**DTOs/** | `Product.cs`
`ProductDto.cs` | **模型定義**：先建立與資料庫對應的 Entity，再定義要在各層間傳遞的 DTO。 |
| **ISpanShop.Repositories** | **Interfaces/**
(根目錄) | `IProductRepository.cs`
`ProductRepository.cs` | **資料存取**：定義介面並實作 CRUD。這是最直接接觸 DbContext 的地方。 |
| **ISpanShop.Services** | **Interfaces/**
(根目錄) | `IProductService.cs`
`ProductService.cs` | **邏輯處理**：呼叫 Repository 拿 Entity，在此進行商業運算並轉換為 DTO 回傳。 |
| **ISpanShop.MVC** | **Models/**`{功能}/`
**Controllers/**
**Views/**`{功能}/` | `Products/ProductIndex**Vm**.cs`
`ProductsController.cs`
`Products/Index.cshtml` | **網頁展示**：將 Dto 包裝進 Vm，由 Controller 交給 View 渲染畫面。**Vm 請依功能分資料夾。** |

**注意** : **MVC內Models資料夾跟Views資料夾在裡面創建檔案前請幫我創建自己的資料夾⇒像我是要寫產品就是創**`Products` 主要依據你們的功能去命名資料夾

**我們的命名ViewModel => 一律寫Vm

注意 : interfaces 如果你們有需要 Interfaces 請依照這個命名幫我創建資料夾**

 **專案都已經做好參考，可以直接呼叫 (寫出表單主要是讓大家可以看一下我加好的參考有哪些) :**

| **專案名稱** | **需要參考的專案(組長已加好，組員不用做)** | **原因說明** |
| --- | --- | --- |
| **1. ISpanShop.Common** | (無) |  |
| **2. ISpanShop.Models** | (無) |  |
| **3. ISpanShop.Repositories** | `ISpanShop.Models`
`ISpanShop.Common` | **資料存取層**。需要引用 `EfModels` 來操作資料庫實體。 |
| **4. ISpanShop.Services** | `ISpanShop.Repositories`
`ISpanShop.Models`
`ISpanShop.Common` | **商務邏輯層**。需要 Repository 的介面，並將實體轉成 `DTOs`。 |
| **5. ISpanShop.MVC** | `ISpanShop.Services`
`ISpanShop.Models`
`ISpanShop.Common` | **展示層**。需要 Service 拿資料，並將 `DTOs` 包裝進內部的 `ViewModels`。 |
| **6. ISpanShop.WebAPI** | `ISpanShop.Services`
`ISpanShop.Models`
`ISpanShop.Common` | **API 接口**。需要 Service 拿資料並直接回傳 `DTOs`。 |

//以上為已完成的MVC後台功能專案資料夾//



//以下為前台功能所使用之ISpanShop-Frontend專案資料夾//

完整結構範例⇒

src/
├── api/                    # API 呼叫層
│   ├── axios.ts           # axios 實例 + 攔截器設定
│   ├── product.ts         # 商品相關 API
│   ├── cart.ts            # 購物車 API
│   ├── order.ts           # 訂單 API
│   ├── member.ts          # 會員 API
│   └── auth.ts            # 登入註冊 API
│
├── assets/
│   ├── images/
│   ├── icons/
│   └── styles/
│       ├── main.scss      # 全站樣式入口
│       ├── _variables.scss # 顏色、字級變數
│       └── _reset.scss
│
├── components/             # 共用元件（不是頁面）
│   ├── common/            # 通用：按鈕、Modal、Loading
│   ├── product/           # 商品相關：ProductCard、ProductGallery
│   ├── cart/              # 購物車相關：CartItem、CartBadge
│   └── form/              # 表單：InputField、SelectField
│
├── composables/            # ⭐ 補充：Vue 3 的 Composition API 邏輯複用
│   ├── useCart.ts
│   ├── usePagination.ts
│   └── useDebounce.ts
│
├── layouts/
│   ├── DefaultLayout.vue  # 前台版型（Header + Footer）
│   ├── MemberLayout.vue   # 會員中心版型（含側邊欄）
│   └── BlankLayout.vue    # 空白版型（登入、註冊頁用）
│
├── router/
│   ├── index.ts
│   └── routes.ts          # 路由清單可以拆出來
│ 
├── stores/                 # Pinia 全域狀態管理
│   ├── cart.ts            # 購物車（電商最重要）
│   ├── auth.ts            # 登入狀態、token
│   ├── member.ts          # 會員資料
│   └── product.ts         # 商品快取（看需求）
│
├── types/                  # TypeScript 型別
│   ├── product.ts
│   ├── order.ts
│   ├── member.ts
│   ├── cart.ts
│   └── api.ts             # API 共用回應格式
│
├── utils/
│   ├── format.ts          # 千分位、日期、價格格式化
│   ├── validator.ts       # 表單驗證
│   └── storage.ts         # localStorage 封裝
│
├── views/                  # 頁面元件
│   ├── home/
│   │   └── HomeView.vue
│   ├── product/
│   │   ├── ProductListView.vue
│   │   └── ProductDetailView.vue
│   ├── cart/
│   │   └── CartView.vue
│   ├── checkout/
│   │   ├── CheckoutView.vue
│   │   └── PaymentResultView.vue
│   ├── member/
│   │   ├── MemberCenterView.vue
│   │   ├── OrderHistoryView.vue
│   │   └── ProfileView.vue
│   ├── auth/
│   │   ├── LoginView.vue
│   │   └── RegisterView.vue
│   └── error/
│       └── NotFoundView.vue
│
├── constants/              # 補充：常數
│   ├── apiPaths.ts        # API 路徑常數
│   └── orderStatus.ts     # 訂單狀態列舉
│
├── App.vue
└── main.ts


總整理表(說明)

| 資料夾 | 說明 | 範例 |
| --- | --- | --- |
| `api/` | 呼叫後端 API 的程式碼 | 取得商品、加入購物車 |
| `assets/` | 圖片和 CSS | logo、商品預設圖 |
| `components/` | 可重複使用的小元件 | 商品卡片、導覽列 |
| `layouts/` | 頁面的外框版型 | 前台版型、會員中心版型 |
| `router/` | 網址對應頁面的設定 | `/cart` → 購物車頁 |
| `stores/` | 全域共享的資料 | 購物車、登入狀態 |
| `types/` | 資料的型別定義 | 商品長什麼樣、訂單長什麼樣 |
| `utils/` | 共用工具函式 | 價格格式化、日期格式化 |
| `views/` | 完整的頁面 | 首頁、商品列表頁 |
| `composables/` | 可複用的邏輯 | 分頁邏輯 |
| `constants/` | 固定常數 | 訂單狀態列舉 |


//以上為前台功能所使用之ISpanShop-Frontend專案資料夾//


目前後台功能已經開發完畢，後台功能以ISpanShop.MVC為主、涵蓋ISpanShop.Common、ISpanShop.Models、ISpanShop.Repositories、ISpanShop.Services等資料夾。

現階段開發目標：
使用 Vue 3 的 Composition API (`<script setup>`) 語法，搭配 TypeScript以及Element-Plus來進行前台功能的開發
(如：前台會員中心、購物車結帳頁面、商品下單等...功能依此類推)，並且採取完全前後端分離的方式來進行開發。

以ISpanShop-Frontend為主，在ISpanShop.MVC裡面的Controller裡面的Api裡面新增能夠和ISpanShop-Frontend溝通的ApiController，
並透過此api去呼叫ISpanShop.MVC內的repository以及service功能，來實現前台功能。

會員功能採用JWT驗證，實現安全性驗證

頁面安全性需求：我的訂單、我的優惠券、購物車頁面等等，這些都是基於會員登入過後才能存取的頁面，也請在設計網址的時候，考慮進去會員加密的需求。

必要時可以參照整個ISpanShop-Core資料夾的內容，尤其是前台的欄位在定義時，盡量以ISpanShop.MVC內的ORM所生成之efmodels及dbcontexts的欄位為參照，保持名稱的開發一致性。

===============================
1. 在會員系統完成前，我們可能是透過 URL 參數（例如 ?memberId=5）來抓資料。
但在真實環境中，這會產生嚴重的安全性漏洞：
越權存取： 登入 A 會員的人，只要手動改網址 ID，就能看到 B 會員的訂單。
信任問題： 前端傳過來的 memberId 是不可信的。
2. 重構的核心做法：從 Token 取得身分既然組員已經完成了登入，通常後端會驗證使用者的身分並發放一個身分憑證（例如 JWT Token）。
前端（Vue 3）：你不再需要手動從外部傳入 memberId。在呼叫 API 時，統一從 Pinia 或 LocalStorage 取得 Token，並放在 Header（例如 Authorization: Bearer <Token>）送出。
後端（Web API）：你的 API 應加上 [Authorize] 屬性。關鍵改變： API 內部不應接收 memberId 參數，而是透過 User.Identity 或 Claims 來解析出目前登入者的 ID。
這樣一來，使用者只能查到「自己的」資料，無法透過竄改參數來存取他人隱私。
3. 進行以下整合：統一請求攔截（Axios Interceptors）： 在 Vue 專案中設定 Axios 攔截器，確保每一次發送請求時，都會自動帶上組員完成的登入 Token。
路由守衛（Navigation Guards）： 在 Vue Router 設定，如果使用者沒登入，就直接導向登入頁，保護你的訂單列表與儀表板不被未授權查看。
後端 Claims 擴充： 確保組員在登入時，有把 MemberId 或 ShopId 放入 Claims 中，這樣你在寫後端 API 時，一行程式碼就能抓到當前使用者。
===============================








