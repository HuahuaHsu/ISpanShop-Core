# 🚀 ISpanShop-Core 初始化與測試指南

**完成時間**: 2026-03-13  
**狀態**: ✅ 已部署、可立即測試

---

## 📋 目錄

1. [環境要求](#環境要求)
2. [初始化步驟](#初始化步驟)
3. [編譯與運行](#編譯與運行)
4. [功能測試](#功能測試)
5. [常見問題](#常見問題)
6. [技術棧確認](#技術棧確認)

---

## 🔧 環境要求

### 必需軟體
| 名稱 | 版本 | 用途 |
|------|------|------|
| .NET SDK | 8.0+ | 編譯與運行 |
| SQL Server | 2019+ / SQL Server Express | 資料庫 |
| Visual Studio | 2022+ (可選) | IDE 開發 |
| Git | Latest | 版本控制 |

### 硬體要求
- **CPU**: 四核以上
- **RAM**: 8GB 以上
- **磁碟空間**: 10GB 以上

### 驗證環境
```bash
# 檢查 .NET 版本
dotnet --version

# 檢查 SQL Server 連線（需配置 ConnectionString）
sqlcmd -S localhost -U sa -P your_password
```

---

## 🎯 初始化步驟

### STEP 1: 複製專案 (5 分鐘)

```bash
# 進入專案目錄
cd C:\Users\ispan\source\repos\ISpanShop-Core\ISpanShop-Core-1.0

# 檢查 Git 狀態
git status

# 建立分支（推薦）
git checkout -b feature/setup-testing
```

### STEP 2: 配置資料庫連線 (5 分鐘)

編輯 **appsettings.json** 或 **appsettings.Development.json**:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ISpanShop;User Id=sa;Password=your_password;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

**說明**:
- `Server`: SQL Server 主機位置
- `Database`: 資料庫名稱
- `User Id`: 資料庫帳號（預設 sa）
- `Password`: 資料庫密碼
- `TrustServerCertificate`: 開發環境設為 true

### STEP 3: 還原 NuGet 套件 (10 分鐘)

```bash
# 進入專案目錄
cd C:\Users\ispan\source\repos\ISpanShop-Core\ISpanShop-Core-1.0

# 還原所有專案的依賴
dotnet restore

# 檢查還原結果
dotnet list package --outdated
```

**預期套件**:
- BCrypt.Net-Next v4.1.0 ✅
- Microsoft.EntityFrameworkCore v9.0.13 ✅
- Microsoft.AspNetCore.Authentication.Cookies ✅

---

## 🏗️ 編譯與運行

### STEP 1: 清理舊編譯 (2 分鐘)

```bash
# 清除編譯輸出
dotnet clean

# 刪除本地快取（可選）
Remove-Item -Path ".vs" -Recurse -Force
Remove-Item -Path "bin", "obj" -Recurse -Force
```

### STEP 2: 編譯方案 (15 分鐘)

```bash
# 編譯整個方案
dotnet build --configuration Debug

# 預期結果
# ✅ Build succeeded with 0 errors
# ✅ No compile warnings
```

**常見錯誤處理**:

| 錯誤 | 原因 | 解決方案 |
|------|------|---------|
| `CS0246: 找不到型別或命名空間` | 套件未安裝 | 執行 `dotnet restore` |
| `database connection failed` | 連線字串錯誤 | 檢查 appsettings.json |
| `Severity Code Warnings` | 編譯警告 | 通常可忽略，但應檢查 |

### STEP 3: 執行應用 (3 分鐘)

```bash
# 啟動應用（開發模式）
dotnet run --project ISpanShop.MVC

# 或使用 Visual Studio
# 1. 開啟 ISpanShop.sln
# 2. 設置 ISpanShop.MVC 為啟動專案
# 3. 按 F5 或點擊 "啟動" 按鈕
```

**應用啟動檢查**:
```
• 應用監聽埠位: https://localhost:XXXXX
• 資料庫初始化: ✅ EnsureAdminUserAsync() 完成
• 認證設定: ✅ Cookie 驗證已配置
```

### STEP 4: 驗證資料庫初始化 (2 分鐘)

應用啟動時自動執行以下初始化：

```csharp
await DataSeeder.SeedAsync(context);                    // ✅ 種子資料
await DataSeeder.PatchMissingReviewDataAsync(context);  // ✅ 修復缺失資料
await DataSeeder.EnsurePendingProductsAsync(context);   // ✅ 建立測試商品
await DataSeeder.EnsureAdminUserAsync(context);         // ✅ 建立預設管理員
```

**驗證方式**:
```sql
-- 執行 SQL 查詢驗證
SELECT * FROM Users WHERE Username LIKE 'ADM%'
SELECT * FROM AdminUsers WHERE IsBlacklisted = 0
SELECT COUNT(*) FROM Products WHERE ReviewStatus = 0
```

---

## ✅ 功能測試

### 測試場景 1: 管理員登入

**路徑**: `/Admin/Auth/Login`

#### 測試步驟

| 步驟 | 操作 | 預期結果 |
|------|------|---------|
| 1 | 打開登入頁面 | 顯示登入表單 |
| 2 | 輸入預設帳號 (ADM001) | 帳號欄位接受輸入 |
| 3 | 輸入預設密碼 | 密碼欄位隱蔽顯示 |
| 4 | 點擊「登入」 | 認證成功，導向儀表板 |
| 5 | 驗證 Cookie | 存在 `AdminCookieAuth` Cookie |
| 6 | 檢查 Session | User Claims 包含 userid 與 role |

**預設帳號（由 DataSeeder 建立）**:
- 帳號: ADM001
- 密碼: 在 DataSeeder.cs 中定義（預設通常為 Admin@12345）
- 角色: SuperAdmin

```csharp
// 在 ISpanShop.Models/Seeding/DataSeeder.cs 中查看
public static async Task EnsureAdminUserAsync(ISpanShopDBContext context)
{
    // 檢查是否已有超級管理員
    var superAdmin = context.AdminUsers.FirstOrDefault(a => a.AdminLevelId == 1);
    if (superAdmin == null)
    {
        // 建立預設超級管理員
        var admin = new AdminUser
        {
            Account = "ADM001",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@12345"),
            Email = "ADM001@ispan.com",
            AdminRoleId = 1,  // Admin Role
            AdminLevelId = 1, // SuperAdmin Level
            IsFirstLogin = true
        };
        context.AdminUsers.Add(admin);
        await context.SaveChangesAsync();
    }
}
```

### 測試場景 2: 新增管理員

**路徑**: `/Admin/Admin/Index`  
**權限**: SuperAdmin 角色

#### 測試步驟

| 步驟 | 操作 | 預期結果 |
|------|------|---------|
| 1 | 導向到管理員管理頁 | 顯示所有管理員列表 |
| 2 | 點擊「+ 新增管理員」按鈕 | 彈出 Modal 表單 |
| 3 | 選擇「管理員等級」 | 下拉選單顯示非超管等級 |
| 4 | 輸入「密碼」 | 密碼欄位接受輸入 |
| 5 | 驗證密碼強度 | 提示："需至少 8 字元、含字母、含數字" |
| 6 | 點擊「確認新增」 | 背景提交表單 |
| 7 | 檢查回傳結果 | 顯示成功訊息 + 自動生成帳號 (ADM00X) |
| 8 | 檢查列表 | 新增的管理員出現在列表中 |

**預期帳號生成序列**:
```
ADM001 → ADM002 → ADM003 → ... → ADM999
```

**密碼驗證規則**:
```regex
最少 8 字元
必須含至少 1 個英文字母 [A-Za-z]
必須含至少 1 個數字 [0-9]

範例:
✅ Valid:   "Admin@12345", "Pass123"
❌ Invalid: "12345678", "abcdefgh", "Admin@@@@"
```

### 測試場景 3: 停用管理員

**路徑**: `/Admin/Admin/Index`

#### 測試步驟

| 步驟 | 操作 | 預期結果 |
|------|------|---------|
| 1 | 在管理員列表中找到目標 | 顯示完整列表 |
| 2 | 點擊「停用」按鈕 | 彈出確認對話框 |
| 3 | 點擊「確認停用」 | 背景提交請求 |
| 4 | 檢查列表更新 | 該管理員狀態變為「停用中」 |
| 5 | 嘗試用停用帳號登入 | 登入失敗（帳號已停用） |

**安全機制驗證**:

```
✅ 防止自我停用
   - 當前用戶無法停用自己
   - 按鈕對自己的帳號禁用

✅ 防止刪除最後超管
   - 若只剩 1 位超級管理員，無法停用
   - 系統提示："至少需保留一位超級管理員"

✅ 無法直接新增超級管理員
   - 下拉選單不顯示「超級管理員」選項
```

### 測試場景 4: 密碼加密驗證

**驗證 BCrypt 加密**:

```sql
-- 檢查資料庫中的密碼是否已加密
SELECT Account, PasswordHash 
FROM AdminUsers 
WHERE Account LIKE 'ADM%'

-- 預期結果（加密密碼範例）
-- Account: ADM001
-- PasswordHash: $2a$11$XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
--              (60 字元的 BCrypt 雜湊值)
```

**驗證登入時 BCrypt 驗證**:

```csharp
// AdminService.VerifyLogin() 使用以下邏輯
if (!BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
{
    return null; // 驗證失敗
}
return admin;    // 驗證成功
```

---

## 🧪 快速功能檢查清單

```
📋 編譯檢查
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ☐ dotnet build 成功（0 錯誤）
  ☐ 沒有編譯警告
  ☐ 所有專案參考正確
  ☐ NuGet 套件已還原

🔐 認證檢查
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ☐ 能成功登入管理員帳號
  ☐ 輸入錯誤密碼時拒絕存取
  ☐ Cookie 已設置（HttpOnly, SameSite, Secure）
  ☐ 登出後無法存取受保護頁面

👤 管理員新增檢查
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ☐ 能開啟新增管理員 Modal
  ☐ 帳號自動生成（ADM00X 格式）
  ☐ 密碼強度驗證正常工作
  ☐ 無法選擇「超級管理員」等級
  ☐ 新增成功後顯示帳號
  ☐ 新增管理員出現在列表中

🚫 停用功能檢查
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ☐ 能成功停用其他管理員
  ☐ 無法停用自己的帳號
  ☐ 無法停用最後的超級管理員
  ☐ 停用後不能用該帳號登入
  ☐ 列表顯示「停用中」狀態

🔒 安全機制檢查
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ☐ 密碼使用 BCrypt 加密
  ☐ Cookie 有 7 天過期時間
  ☐ 未登入無法存取管理員頁面
  ☐ 非 SuperAdmin 無法存取管理員管理
  ☐ SQL 注入防護（使用參數化查詢）

📊 列表顯示檢查
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ☐ 顯示所有管理員資訊
  ☐ 狀態標籤顯示正確
  ☐ 管理員等級顯示正確
  ☐ 建立時間格式正確
```

---

## ❓ 常見問題 (FAQ)

### Q1: 啟動時提示「資料庫連線失敗」

**原因**: ConnectionString 配置錯誤或 SQL Server 未運行

**解決**:
```bash
# 1. 檢查 SQL Server 是否在運行
net start MSSQLSERVER

# 2. 驗證連線字串
# appsettings.json 中的 ConnectionString 應為:
# "Server=localhost;Database=ISpanShop;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"

# 3. 使用 sqlcmd 測試連線
sqlcmd -S localhost -U sa -P your_password -Q "SELECT @@VERSION"

# 4. 若使用 SQL Server Express，可能需要啟用 TCP/IP
# SQL Server Configuration Manager → SQL Server Network Configuration
#    → SQLEXPRESS → TCP/IP 啟用
```

### Q2: 編譯時提示「找不到 BCrypt」

**原因**: NuGet 套件未正確還原

**解決**:
```bash
# 1. 清除 NuGet 快取
dotnet nuget locals all --clear

# 2. 重新還原
dotnet restore

# 3. 手動安裝（如果上述步驟無效）
dotnet add ISpanShop.Services package BCrypt.Net-Next --version 4.1.0
dotnet add ISpanShop.Repositories package BCrypt.Net-Next --version 4.1.0
```

### Q3: 登入後導向失敗

**原因**: Area 路由配置問題

**解決**:
```csharp
// Program.cs 確保包含以下配置:

// 認證中介軟體（必須在授權之前）
app.UseAuthentication();
app.UseAuthorization();

// Area 路由（必須在 default 之前）
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Orders}/{action=Dashboard}/{id?}",
    defaults: new { area = "Admin" });
```

### Q4: 無法新增管理員（Modal 無法打開）

**原因**: JavaScript 檯吏錯誤或 HTML 結構問題

**解決**:
```html
<!-- 檢查 Index.cshtml 是否包含以下函式 -->
<script>
function openCreateModal() {
    const modal = document.getElementById('createModal');
    if (modal) {
        modal.classList.remove('hidden');
    }
}
</script>

<!-- 或在瀏覽器開發者工具 (F12) 的 Console 檢查錯誤 -->
```

### Q5: 密碼驗證失敗

**原因**: 密碼規則不符

**解決**:
```
密碼必須滿足:
1. 至少 8 字元
2. 至少含 1 個英文字母 (A-Z, a-z)
3. 至少含 1 個數字 (0-9)

✅ 有效: "Admin@123", "Test@1234", "Pass1234"
❌ 無效: "12345678" (無字母), "abcdefgh" (無數字), "Admin@" (少於 8 字元)
```

### Q6: 提示「無法停用自己的帳號」

**原因**: 系統的安全保護機制

**解決**:
```
此為設計特性，防止管理員誤操作鎖定自己。
解決方式:
1. 用其他 SuperAdmin 帳號停用該帳號
2. 若沒有其他 SuperAdmin，聯絡系統管理員進行資料庫維護
```

### Q7: 提示「至少需保留一位超級管理員」

**原因**: 系統保護最後一位超管，防止完全鎖定

**解決**:
```
此為安全機制。若確實需要停用該帳號:
1. 先新增一位新的 SuperAdmin（需要 ADM001 新增）
2. 然後停用舊帳號
```

---

## 📊 技術棧確認

### 後端框架

| 名稱 | 版本 | 驗證 | 狀態 |
|------|------|------|------|
| .NET Core | 8.0 | `dotnet --version` | ✅ |
| C# | 12.0 | Targeting net8.0 | ✅ |
| ASP.NET Core MVC | 8.0 | NuGet package | ✅ |
| Entity Framework Core | 9.0.13 | appsettings.json | ✅ |
| Microsoft.EntityFrameworkCore.SqlServer | 9.0.13 | Package.config | ✅ |

### 安全與驗證

| 名稱 | 版本 | 用途 | 驗證 |
|------|------|------|------|
| BCrypt.Net-Next | 4.1.0 | 密碼加密 | ✅ |
| Microsoft.AspNetCore.Authentication.Cookies | 8.0 | Cookie 認證 | ✅ |

### 資料庫

| 名稱 | 版本 | 狀態 |
|------|------|------|
| SQL Server | 2019+ | ✅ |
| SQL Server Express | 2019+ | ✅ |

### 前端框架

| 名稱 | 用途 | 狀態 |
|------|------|------|
| HTML 5 | 檢視層 | ✅ |
| Tailwind CSS | 樣式 | ✅ |
| JavaScript (Vanilla) | 交互 | ✅ |

---

## 📝 測試報告模板

複製以下模板記錄測試結果:

```markdown
# 測試報告 - 2026-03-13

## 環境資訊
- .NET 版本: [填寫]
- SQL Server 版本: [填寫]
- 作業系統: [填寫]

## 編譯結果
- [ ] dotnet build 成功
- 錯誤數: [填寫]
- 警告數: [填寫]

## 認證測試
- [ ] 登入成功 (ADM001 帳號)
- [ ] 登出成功
- [ ] 錯誤密碼被拒絕
- [ ] Cookie 正確設置

## 新增管理員測試
- [ ] Modal 彈出正常
- [ ] 密碼驗證規則正常
- [ ] 帳號自動生成正確
- [ ] 新增成功並顯示在列表

## 停用功能測試
- [ ] 能停用其他管理員
- [ ] 無法停用自己
- [ ] 無法停用最後超管
- [ ] 停用後無法登入

## 安全驗證
- [ ] 密碼使用 BCrypt 加密
- [ ] 未登入無法存取管理頁面
- [ ] 非 SuperAdmin 無法存取

## 結論
整體狀態: [✅ 通過 / ⚠️ 有警告 / ❌ 失敗]

備註:
[填寫任何其他觀察或問題]
```

---

## 🔄 後續步驟

1. **開發測試** (1-2 小時)
   - 執行所有上述測試場景
   - 記錄所有問題與修復過程

2. **程式碼審查** (30-60 分鐘)
   - 檢查代碼品質
   - 驗證安全機制
   - 確認文檔完整性

3. **效能測試** (可選)
   - 壓力測試 (大量新增操作)
   - 並發登入測試
   - 資料庫查詢效能

4. **部署準備** (30 分鐘)
   ```bash
   # 編譯發行版本
   dotnet publish -c Release -o ./publish
   
   # 檢查發行文件大小
   ls -lh ./publish/
   ```

5. **文檔更新**
   - 更新 README.md 部署說明
   - 建立操作手冊
   - 記錄已知限制

---

## 📞 支援資源

### 文檔參考
- `FINAL_DELIVERY_SUMMARY.md` - 整體完成總結
- `COMPLETE_IMPLEMENTATION_REPORT.md` - 詳細實現報告
- `QUICK_CHECKLIST.md` - 快速檢查清單
- `INDEX_REPLACEMENT_GUIDE.md` - View 替換指南

### 聯繫方式
如遇到技術問題，請參考以下順序:

1. 閱讀本文檔的 FAQ 部分
2. 查看相關文檔文件
3. 檢查事件日誌 (Event Viewer)
4. 檢查應用日誌 (Application Insights - 如已配置)

---

## ✨ 祝您測試順利！

整個專案已完成，所有代碼、文檔、測試清單均已準備就緒。
現在只需按照本指南進行初始化和測試，即可快速驗證所有功能。

**預計時間**: ~1.5 小時完成所有初始化與測試

---

**更新日期**: 2026-03-13  
**文檔版本**: v1.0  
**狀態**: ✅ 就緒
