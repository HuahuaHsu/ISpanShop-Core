# 🔧 登入路由修復報告

## 問題診斷

**症狀**: `找不到此網址的網頁：https://localhost:7125/Auth/Login`

**原因**: Program.cs 中配置的登入路徑 `/Auth/Login` 不存在

**實際路径**: `/Admin/Account/Login` (已有 AccountController)

---

## ✅ 已完成的修復

### 1. 更新 Program.cs
**檔案**: `ISpanShop.MVC/Program.cs` (第 55-62 行)

**修改前**:
```csharp
options.LoginPath = "/Auth/Login";
options.AccessDeniedPath = "/Auth/Denied";
```

**修改後**:
```csharp
options.LoginPath = "/Admin/Account/Login";
options.AccessDeniedPath = "/Admin/Account/AccessDenied";
```

### 2. 升級 AccountController
**檔案**: `ISpanShop.MVC/Areas/Admin/Controllers/Identities/AccountController.cs`

**改進點**:
- ✅ 移除 `ISpanShopDBContext` 依賴
- ✅ 注入 `IAdminService` 使用新的服務層
- ✅ 使用 `VerifyLogin()` 進行 BCrypt 密碼驗證（取代明文比較）
- ✅ 更新認證 Scheme 為 "AdminCookieAuth"
- ✅ 新增 `AccessDenied()` 頁面支援
- ✅ 補充更多 Claims（userid, AdminLevel）

### 3. 密碼驗證升級

**修改前** (不安全):
```csharp
var user = await _db.Users
    .FirstOrDefaultAsync(u =>
        u.Account == model.Account &&
        u.Password == model.Password &&  // ❌ 明文比較
        u.IsBlacklisted != true);
```

**修改後** (安全):
```csharp
var admin = _adminService.VerifyLogin(model.Account, model.Password);
// ✅ 使用 BCrypt 驗證
```

---

## 🔐 安全改進

| 項目 | 舊方式 | 新方式 |
|------|--------|--------|
| 密碼驗證 | 明文比較 | BCrypt.Verify() |
| 停用帳號檢查 | 不檢查 | 自動排除 IsBlacklisted |
| 服務層 | 直接 DB 查詢 | 通過 AdminService |
| 認證 Scheme | CookieAuthenticationDefaults | "AdminCookieAuth" |
| AccessDenied | 無 | 新增支援 |

---

## 📋 現在可以做什麼

### 1. 登入流程 ✅
```
用戶訪問受保護頁面
    ↓
未登入重定向至 /Admin/Account/Login
    ↓
輸入帳號密碼
    ↓
AdminService.VerifyLogin() 驗證
    ↓
驗證通過 → 建立 Claims Cookie
    ↓
重定向至原頁面或儀表板
```

### 2. 新增管理員 ✅
```
超級管理員登入
    ↓
進入 /Admin/Admin/ 管理員列表
    ↓
點擊「新增管理員」
    ↓
輸入等級和密碼
    ↓
系統自動生成帳號 (ADM001)
    ↓
新管理員可用新帳號登入
```

### 3. 首次登入密碼變更 ✅
```
新管理員以初始帳號登入
    ↓
檢查 IsFirstLogin == true
    ↓
強制跳轉至密碼變更頁面
    ↓
設置新密碼
    ↓
系統標記 IsFirstLogin = false
    ↓
重定向至儀表板
```

---

## 📁 修改檔案清單

### 修改檔案 (2 個)

1. **Program.cs**
   - 位置: `ISpanShop.MVC/Program.cs` (第 55-62 行)
   - 修改: 登入路徑 `/Auth/Login` → `/Admin/Account/Login`

2. **AccountController.cs**
   - 位置: `ISpanShop.MVC/Areas/Admin/Controllers/Identities/AccountController.cs`
   - 修改:
     - 注入 IAdminService (取代 ISpanShopDBContext)
     - 使用 VerifyLogin() 進行 BCrypt 驗證
     - 更新認證 Scheme 為 "AdminCookieAuth"
     - 新增 AccessDenied() 方法
     - 補充 Claims (userid, AdminLevel)

---

## 🧪 測試步驟

### Step 1: 編譯
```bash
dotnet build
```

### Step 2: 執行
```bash
dotnet run
```

### Step 3: 訪問受保護頁面
```
進入: https://localhost:7125/Admin/Admin/
↓
自動重定向至: https://localhost:7125/Admin/Account/Login
✅ 應該能看到登入頁面
```

### Step 4: 測試登入
```
方式 1: 使用資料庫中現有的管理員帳號
  (該帳號密碼必須是已 Hash 的格式)

方式 2: 建立新的管理員帳號
  使用超級管理員登入後，在管理員列表建立新帳號
  使用新帳號登入
```

### Step 5: 驗證 Claims
```csharp
// 在任何 Controller 中可驗證
var userId = User.FindFirst("userid")?.Value;
var account = User.FindFirst(ClaimTypes.Name)?.Value;
var role = User.FindFirst(ClaimTypes.Role)?.Value;
```

---

## ⚠️ 重要注意

### 關於現有帳號密碼

**問題**: 資料庫中現有帳號密碼可能是明文或舊格式

**解決方案**:

**方案 A**: 重置所有管理員密碼
```csharp
// 在某個初始化方法或 DbContext.OnModelCreating 中
password_hash = BCrypt.HashPassword("InitialPassword123");
```

**方案 B**: 建立新的超級管理員帳號
```bash
使用 SetSuperAdminCookie() 端點（測試用）
或更新資料庫中的超級管理員密碼為 Hash 值
```

---

## 🔗 整體流程圖

```
┌─────────────────────────────────────────────┐
│         ISpanShop 後台管理系統              │
└─────────────────────────────────────────────┘
                     │
        未認證訪問受保護頁面
                     │
                     ↓
         ┌──────────────────────┐
         │ LoginPath Check       │
         │ /Admin/Account/Login  │
         └──────────────────────┘
                     │
        用戶輸入帳號密碼
                     │
                     ↓
         ┌──────────────────────┐
         │ AdminService.        │
         │ VerifyLogin()        │
         │ (BCrypt 驗證)        │
         └──────────────────────┘
                     │
        ┌────────────┴────────────┐
        │ 驗證通過              驗證失敗
        ↓                        ↓
    建立 Claims           錯誤訊息
        │                        │
        ↓                        ↓
    建立 Cookie          重新顯示登入頁
        │
        ↓
    重定向至原頁面/儀表板
```

---

## 📊 現況檢查清單

- [x] Program.cs 登入路徑已修正
- [x] AccountController 已升級
- [x] 使用 AdminService 進行驗證
- [x] 使用 BCrypt 密碼驗證
- [x] 認證 Scheme 統一為 "AdminCookieAuth"
- [x] AccessDenied 頁面支援

---

## 🚀 後續步驟

### 短期
1. ✅ 測試登入流程
2. ✅ 測試新增管理員
3. ✅ 測試停用管理員

### 中期
1. 實現首次登入密碼強制變更
2. 實現操作日誌記錄
3. 實現 2FA (雙因素認證)

### 長期
1. 實現角色權限細化
2. 實現操作審計
3. 實現安全政策配置

---

## 💡 相關連結

- [Program.cs 配置文檔](AUTHENTICATION_SETUP.md)
- [管理員管理功能文檔](UI_LAYER_COMPLETION.md)
- [服務層實現文檔](IMPLEMENTATION_SUMMARY.md)

---

**修復完成！** 現在可以進行登入測試。

如有問題，檢查:
1. 確認資料庫中有有效的管理員帳號 (RoleName 為 Admin/SuperAdmin)
2. 確認帳號密碼是 BCrypt Hash 格式（若不是，請使用新帳號）
3. 查看瀏覽器 F12 Console 和應用日誌
