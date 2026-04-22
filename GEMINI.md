//以下為後台功能所使用所有專案資料夾//

**ISpanShop.sln (解決方案)**
├── ISpanShop.Common         # 類別庫 (基礎建設)
│   ├── Enums/              # 全專案共用列舉 (OrderStatus, MemberLevel, Role)
│   ├── Helpers/            # 輔助工具 (SecurityHelper, ClaimsExtensions, EmailService)
│   └── EcpayHelper.cs      # 綠界支付輔助類別
│
├── ISpanShop.Models         # 類別庫 (資料模型定義)
│   ├── EfModels/           # Entity Framework Core 資料庫實體 (DB First 生成)
│   ├── DTOs/               # 資料傳輸物件 (依功能分資料夾)
│   │   ├── Auth/           # 登入、註冊、變更密碼 DTO
│   │   ├── Members/        # 會員、錢包、點數相關 DTO
│   │   ├── Products/       # 商品資訊、分類、規格 DTO
│   │   └── Orders/         # 訂單相關、支付請求 DTO
│   └── Seeding/            # 資料初始化 Seed Data
│
├── ISpanShop.Repositories   # 類別庫 (資料存存取層)
│   ├── Members/            # 會員、使用者倉儲 (IUserRepository)
│   ├── Products/           # 商品、分類、庫存倉儲
│   ├── Orders/             # 訂單、評論倉儲
│   └── Stores/             # 賣場、申請紀錄倉儲
│
├── ISpanShop.Services       # 類別庫 (商業邏輯層)
│   ├── Auth/               # 認證邏輯 (IFrontAuthService)、JWT 簽發
│   ├── Members/            # 會員帳號邏輯 (IAccountService)、錢包
│   ├── Products/           # 商品邏輯、分類管理、庫存控管
│   ├── Orders/             # 結帳流程、訂單狀態管理
│   └── Payments/           # 金流串接 (綠界、藍新)
│
├── ISpanShop.MVC            # ASP.NET Core MVC (管理後台 + API 介面)
│   ├── Areas/Admin/        # 管理員後台系統
│   ├── Controllers/Api/    # 前台專用 API 控制器 (與 Vue 3 對接)
│   │   ├── FrontAuthController.cs    # 會員認證
│   │   ├── FrontMemberController.cs  # 會員中心
│   │   └── FrontStoreController.cs   # 賣場申請
│   ├── Hubs/               # SignalR 即時通訊 (ChatHub)
│   ├── Middleware/         # 全域例外處理、身分驗證攔截
│   ├── Models/             # 後台 ViewModel (**Vm)
│   └── Program.cs          # 程式啟動與 DI 服務註冊中心

**注意** : 關於Model，請依照位置創建檔案⇒

ISpanShop.Models ⇒ **EfModels**、**DTOs**
ISpanShop.MVC ⇒ **Models ⇒ ViewModel (依功能分資料夾，命名為 **Vm)**

**ISpanShop 全專案開發流程規範表**

| **專案名稱** | **職責與開發重點** | **關鍵技術/目錄** |
| --- | --- | --- |
| **ISpanShop.Common** | 基礎建設與工具類 | `Enums/`, `Helpers/` |
| **ISpanShop.Models** | 實體與資料傳輸物件 | `EfModels/`, `DTOs/` |
| **ISpanShop.Repositories** | 直接存取資料庫，不含商業邏輯 | `Interfaces/`, `Implementations/` |
| **ISpanShop.Services** | 核心邏輯處理，呼叫 Repo 並轉換為 DTO | `Interfaces/`, `Services/` |
| **ISpanShop.MVC** | 後台介面 (Razor) 與 前台 API (WebAPI) | `Areas/Admin/`, `Controllers/Api/` |

**專案參考關係說明** :
- `MVC/WebAPI` -> `Services` -> `Repositories` -> `Models`
- `Common` 被所有專案參考。

//以上為後台功能所使用所有專案資料夾//



//以下為前台功能所使用之ISpanShop-Frontend專案資料夾//

src/
├── api/                    # API 呼叫層 (Axios 封裝)
│   ├── auth.ts            # 登入、註冊、密碼重設
│   ├── member.ts          # 個人資料、地址、點數錢包
│   ├── product.ts         # 商品列表、詳情、評價
│   ├── cart.ts            # 購物車管理
│   ├── order.ts           # 訂單查詢、下單、退貨
│   ├── store.ts           # 賣場資訊、賣家申請
│   ├── seller.ts          # 賣家後台管理 (商品、報表)
│   ├── chat.ts            # 聊天系統 API
│   └── axios.ts           # Axios 實例與攔截器設定
│
├── components/             # 共用元件庫
│   ├── common/            # 通用：AuthDialog, Loading, Empty
│   ├── product/           # 商品相關：ProductCard, FilterSidebar
│   ├── order/             # 訂單相關：OrderAction, StatusSteps
│   ├── chat/              # 聊天系統：ChatFloating, MessageList
│   └── seller/            # 賣家組件：CategorySelector, ImageUploader
│
├── layouts/                # 頁面佈局版型
│   ├── DefaultLayout.vue  # 前台通用版型 (含 Header/Footer)
│   ├── MemberLayout.vue   # 會員中心版型 (側邊選單)
│   ├── SellerLayout.vue   # 賣家中心專用版型
│   └── BlankLayout.vue    # 獨立頁面 (登入/註冊/重設密碼)
│
├── stores/                 # Pinia 狀態管理
│   ├── auth.ts            # 登入狀態、Token、權限
│   ├── cart.ts            # 購物車暫存
│   └── chat.ts            # 聊天訊息、未讀通知
│
├── views/                  # 頁面主體 (依功能分類)
│   ├── auth/               # 認證相關
│   │   ├── LoginView.vue           # 登入頁
│   │   ├── RegisterView.vue        # 註冊頁
│   │   ├── ForgotPasswordView.vue  # 忘記密碼
│   │   └── ResetPasswordView.vue   # 重設密碼
│   ├── member/             # 會員中心 (需登入)
│   │   ├── MemberCenterView.vue    # 會員中心主頁
│   │   ├── ProfileView.vue         # 個人資料修改
│   │   ├── PasswordView.vue        # 修改密碼
│   │   ├── AddressView.vue         # 收件地址管理
│   │   ├── OrdersView.vue          # 訂單列表
│   │   ├── OrderDetailView.vue     # 訂單詳情
│   │   ├── RefundView.vue          # 退貨/退款申請
│   │   ├── RefundDetailView.vue    # 退貨詳情
│   │   ├── WalletView.vue          # 我的錢包 (點數/餘額)
│   │   ├── MemberCouponView.vue    # 我的優惠券
│   │   ├── SupportTicketView.vue   # 客服諮詢單
│   │   ├── MyStoreView.vue         # 我的賣場狀態 (跳轉進入賣家中心)
│   │   ├── SellerApplyView.vue     # 賣家資格申請
│   │   ├── SettingsView.vue        # 帳號設定
│   │   └── PrivacyView.vue         # 隱私權設定
│   ├── seller/             # 賣家管理後台 (需賣家身分)
│   │   ├── DashboardView.vue       # 賣家儀表板 (概況)
│   │   ├── ProductListView.vue     # 商品管理列表
│   │   ├── ProductEditView.vue     # 新增/編輯商品
│   │   ├── SalesReportView.vue     # 銷售統計報表
│   │   ├── StoreSettingView.vue    # 賣場資訊設定
│   │   └── TodoView.vue            # 待辦事項清單
│   ├── cart/               # 購物流程
│   │   ├── CartView.vue            # 購物車清單
│   │   ├── CheckoutView.vue        # 結帳確認頁
│   │   └── PaymentResultView.vue   # 支付結果導向 (綠界/藍新)
│   ├── error/              # 錯誤處理
│   │   ├── NotFoundView.vue        # 404 頁面
│   │   └── WipView.vue             # 功能開發中 (Work In Progress)
│   ├── HomeView.vue        # 商城首頁
│   ├── ProductsView.vue    # 全站商品列表 / 搜尋結果
│   ├── ProductDetailView.vue # 商品詳情頁
│   ├── CouponsView.vue     # 領券中心
│   └── AboutView.vue       # 關於我們
│
├── types/                  # TypeScript 型別定義 (與後端 DTO 一致)
├── utils/                  # 工具函式 (storage, format, validator)
├── styles/                 # 全域樣式與主題 (Element Plus 定制)
└── router/                 # 路由設定 (路由守衛、權限校驗)

總整理表 (功能對照)

| 資料夾 | 說明 | 關鍵實作範例 |
| --- | --- | --- |
| `api/` | 呼叫後端 API 的程式碼 | `auth.ts` (登入), `order.ts` (下單) |
| `components/` | 可重複使用的小元件 | `ProductCard.vue`, `ChatFloating.vue` |
| `layouts/` | 頁面的外框版型 | `MemberLayout.vue` (會員中心側邊欄) |
| `stores/` | 全域共享的資料 | `auth.ts` (Token), `cart.ts` (購物車內容) |
| `views/` | 完整的頁面元件 | `member/OrdersView.vue`, `seller/DashboardView.vue` |
| `types/` | 資料的型別定義 | `product.ts`, `auth.ts` (與後端 DTO 對應) |
| `utils/` | 共用工具函式 | `storage.ts` (localStorage), `format.ts` (價格格式化) |

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


//以下為專案資料庫結構 (參考EfModel並在每個檔案備註所有欄位)//

- **Address.cs**: `Id` (int), `UserId` (int), `RecipientName` (string), `RecipientPhone` (string), `City` (string), `Region` (string), `Street` (string), `IsDefault` (bool?)
- **AdminLevel.cs**: `Id` (int), `LevelName` (string), `Description` (string)
- **BlacklistRecord.cs**: `Id` (int), `BlockedUserId` (int), `AdminUserId` (int), `Reason` (string), `CreatedAt` (DateTime?), `UnblockAt` (DateTime?)
- **Brand.cs**: `Id` (int), `Name` (string), `Description` (string), `LogoUrl` (string), `Sort` (int?), `IsVisible` (bool?), `IsDeleted` (bool?)
- **Cart.cs**: `Id` (int), `UserId` (int)
- **CartItem.cs**: `Id` (int), `CartId` (int), `StoreId` (int), `ProductId` (int), `VariantId` (int), `Quantity` (int), `UnitPrice` (decimal?)
- **Category.cs**: `Id` (int), `Name` (string), `ParentId` (int?), `IconUrl` (string), `Sort` (int?), `IsVisible` (bool?), `Icon` (string), `NameEn` (string)
- **CategoryAttribute.cs**: `Id` (int), `Name` (string), `InputType` (string), `IsRequired` (bool), `IsActive` (bool), `AllowCustomInput` (bool)
- **CategoryAttributeMapping.cs**: `CategoryId` (int), `CategoryAttributeId` (int), `IsFilterable` (bool), `Sort` (int)
- **CategoryAttributeOption.cs**: `Id` (int), `CategoryAttributeId` (int), `OptionName` (string), `SortOrder` (int)
- **ChatMessage.cs**: `Id` (long), `SessionId` (Guid), `SenderId` (int), `ReceiverId` (int), `Content` (string), `Type` (byte), `IsRead` (bool?), `SentAt` (DateTime?)
- **Coupon.cs**: `Id` (int), `StoreId` (int), `SellerId` (int), `CouponCode` (string), `Title` (string), `DistributionType` (byte), `CouponType` (byte), `DiscountValue` (decimal), `MinimumSpend` (decimal), `MaximumDiscount` (decimal?), `StartTime` (DateTime), `EndTime` (DateTime), `TotalQuantity` (int), `UsedQuantity` (int), `PerUserLimit` (int), `ApplyToAll` (bool), `IsExclusive` (bool), `Status` (byte), `IsDeleted` (bool), `UpdatedBy` (int), `UpdatedAt` (DateTime)
- **CouponCategory.cs**: `CouponId` (int), `CategoryId` (int)
- **LoginHistory.cs**: `Id` (int), `UserId` (int?), `AttemptedAccount` (string), `Ipaddress` (string), `LoginTime` (DateTime), `IsSuccess` (bool)
- **MemberCoupon.cs**: `Id` (int), `UserId` (int), `CouponId` (int), `UsageStatus` (byte), `OrderId` (long?), `ReceivedAt` (DateTime), `UsedAt` (DateTime?)
- **MemberProfile.cs**: `Id` (int), `UserId` (int), `LevelId` (int), `FullName` (string), `Gender` (byte?), `DateOfBirth` (DateOnly?), `PhoneNumber` (string), `TotalSpending` (decimal?), `PointBalance` (int?), `EmailNotification` (bool?), `UpdatedAt` (DateTime?), `IsSeller` (bool?), `AvatarUrl` (string)
- **MembershipLevel.cs**: `Id` (int), `LevelName` (string), `MinSpending` (decimal), `DiscountRate` (decimal)
- **Order.cs**: `Id` (long), `OrderNumber` (string), `UserId` (int), `StoreId` (int), `TotalAmount` (decimal), `ShippingFee` (decimal?), `PointDiscount` (int?), `DiscountAmount` (decimal?), `FinalAmount` (decimal), `Status` (byte?), `PaymentDate` (DateTime?), `RecipientName` (string), `RecipientPhone` (string), `RecipientAddress` (string), `Note` (string), `CreatedAt` (DateTime?), `CompletedAt` (DateTime?), `CouponId` (int?)
- **OrderDetail.cs**: `Id` (long), `OrderId` (long), `ProductId` (int), `VariantId` (int?), `ProductName` (string), `VariantName` (string), `SkuCode` (string), `CoverImage` (string), `Price` (decimal?), `Quantity` (int), `AllocatedDiscountAmount` (decimal?)
- **OrderReview.cs**: `Id` (int), `UserId` (int), `OrderId` (long), `Rating` (byte), `Comment` (string), `StoreReply` (string), `IsHidden` (bool?), `CreatedAt` (DateTime?)
- **PasswordResetToken.cs**: `Id` (int), `UserId` (int), `Token` (string), `ExpiryDate` (DateTime), `IsUsed` (bool), `CreatedAt` (DateTime)
- **PaymentLog.cs**: `Id` (long), `OrderId` (long), `MerchantTradeNo` (string), `TradeNo` (string), `RtnCode` (int?), `RtnMsg` (string), `TradeAmt` (decimal?), `PaymentType` (string), `PaymentDate` (DateTime?), `CreatedAt` (DateTime?)
- **Permission.cs**: `Id` (int), `PermissionKey` (string), `DisplayName` (string), `Description` (string)
- **PointHistory.cs**: `Id` (long), `UserId` (int), `OrderNumber` (string), `ChangeAmount` (int), `BalanceAfter` (int), `Description` (string), `CreatedAt` (DateTime?)
- **Product.cs**: `Id` (int), `StoreId` (int), `CategoryId` (int), `BrandId` (int?), `Name` (string), `Description` (string), `VideoUrl` (string), `SpecDefinitionJson` (string), `MinPrice` (decimal?), `MaxPrice` (decimal?), `TotalSales` (int?), `ViewCount` (int?), `Status` (byte?), `CreatedAt` (DateTime?), `UpdatedAt` (DateTime?), `RejectReason` (string), `IsDeleted` (bool), `ReviewStatus` (int), `ReviewedBy` (string), `ReviewDate` (DateTime?), `ForceOffShelfReason` (string), `ForceOffShelfDate` (DateTime?), `ForceOffShelfBy` (int?), `ReApplyDate` (DateTime?)
- **ProductImage.cs**: `Id` (int), `ProductId` (int), `VariantId` (int?), `ImageUrl` (string), `IsMain` (bool?), `SortOrder` (int?)
- **ProductVariant.cs**: `Id` (int), `ProductId` (int), `SkuCode` (string), `VariantName` (string), `SpecValueJson` (string), `Price` (decimal), `Stock` (int?), `SafetyStock` (int?), `IsDeleted` (bool?)
- **Promotion.cs**: `Id` (int), `Name` (string), `Description` (string), `PromotionType` (int), `StartTime` (DateTime), `EndTime` (DateTime), `Status` (int), `SellerId` (int), `ReviewedBy` (int?), `ReviewedAt` (DateTime?), `RejectReason` (string), `CreatedAt` (DateTime), `UpdatedAt` (DateTime?), `IsDeleted` (bool)
- **PromotionItem.cs**: `Id` (int), `PromotionId` (int), `ProductId` (int), `OriginalPrice` (decimal), `DiscountPrice` (decimal?), `DiscountPercent (int?)`, `QuantityLimit` (int?), `StockLimit` (int?), `SoldCount` (int)
- **PromotionRule.cs**: `Id` (int), `PromotionId` (int), `RuleType` (int), `Threshold (decimal)`, `DiscountType` (int), `DiscountValue` (decimal)
- **ReturnRequest.cs**: `Id` (long), `OrderId` (long), `ReasonCategory` (string), `ReasonDescription` (string), `RefundAmount` (decimal), `Status` (byte), `AdminRemark` (string), `CreatedAt` (DateTime), `UpdatedAt` (DateTime?)
- **ReturnRequestImage.cs**: `Id` (long), `ReturnRequestId` (long), `ImageUrl` (string), `CreatedAt` (DateTime)
- **ReviewImage.cs**: `Id` (int), `ReviewId` (int), `ImageUrl` (string)
- **Role.cs**: `Id` (int), `RoleName` (string), `Description` (string)
- **SensitiveWord.cs**: `Id` (int), `Word` (string), `Category` (string), `IsActive` (bool?), `CreatedTime` (DateTime?), `CategoryId` (int?)
- **SensitiveWordCategory.cs**: `Id` (int), `Name` (string)
- **Store.cs**: `Id` (int), `UserId` (int), `StoreName` (string), `Description` (string), `IsVerified` (bool?), `CreatedAt` (DateTime?), `StoreStatus` (byte), `LogoUrl` (string)
- **SupportTicket.cs**: `Id` (int), `UserId` (int), `OrderId` (long?), `Category` (byte), `Subject` (string), `Description` (string), `AttachmentUrl` (string), `Status` (byte?), `AdminReply` (string), `CreatedAt` (DateTime?), `ResolvedAt` (DateTime?)
- **User.cs**: `Id` (int), `RoleId` (int), `Account` (string), `Password` (string), `Email` (string), `Provider` (string), `ProviderId` (string), `IsConfirmed` (bool?), `ConfirmCode` (string), `IsBlacklisted` (bool?), `CreatedAt` (DateTime?), `UpdatedAt` (DateTime?), `AdminLevelId` (int?), `IsFirstLogin` (bool)

//以上為專案資料庫結構 (參考EfModel並在每個檔案備註所有欄位)//
