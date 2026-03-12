# STEP 5 展示層 (ISpanShop.MVC) - 管理員管理頁面 完成報告

## ✅ 已完成項目

### 1. 更新 AdminIndexVm ✅
**位置**: `ISpanShop.MVC/Areas/Admin/Models/Admins/AdminIndexVm.cs`

新增屬性：
```csharp
public AdminCreateVm CreateForm { get; set; } = new AdminCreateVm();
public string GeneratedAccount { get; set; }
```

保留現有屬性：
- `List<AdminDto> Admins`
- `string Message`
- `List<AdminPermissionDto> PermissionOptions`

### 2. 建立 AdminCreateVm ✅
**位置**: `ISpanShop.MVC/Areas/Admin/Models/Admins/AdminCreateVm.cs`

內容：
```csharp
public string Password { get; set; }                    // 初始密碼
public int AdminLevelId { get; set; }                  // 管理員等級
public List<AdminLevelDto> AdminLevelOptions { get; set; } // 等級選項列表
```

### 3. 更新 AdminsController ✅
**位置**: `ISpanShop.MVC/Areas/Admin/Controllers/Identities/AdminController.cs`

#### GET Index()
- ✅ 取得所有管理員：`_adminService.GetAllAdmins()`
- ✅ 取得權限選項：`_adminService.GetAllPermissions()`
- ✅ 取得可選擇的等級：`_adminService.GetSelectableAdminLevels()`
- ✅ 組成 AdminIndexVm 回傳

#### POST CreateAdmin(AdminCreateVm form)
- ✅ 模型驗證檢查
- ✅ 轉換為 AdminCreateDto
- ✅ 呼叫 `AdminService.CreateAdmin()`
- ✅ 成功：存入 TempData["GeneratedAccount"] 和 TempData["Message"]
- ✅ 失敗：存入錯誤訊息
- ✅ 返回 RedirectToAction("Index")

#### POST DeactivateAdmin(int userId)
- ✅ 從 Claims 取得 currentUserId
- ✅ 呼叫 `AdminService.DeactivateAdmin(userId, currentUserId)`
- ✅ 存入結果訊息至 TempData["Message"]
- ✅ 返回 RedirectToAction("Index")

#### 保留現有方法
- SetSuperAdminCookie - 測試用
- ClearSuperAdminCookie - 測試用
- UpdateRole - 角色更新

### 4. 更新 Index.cshtml ✅
**位置**: `ISpanShop.MVC/Areas/Admin/Views/Admin/Index_new.cshtml`

**已完成功能**：

#### 新增管理員 Modal（★ 兩個輸入欄位）
✅ 初始密碼輸入欄
  - 類型：password
  - 最小長度：8 字元
  - placeholder 提示

✅ 管理員等級下拉選單
  - 來源：AdminLevelOptions
  - 自動排除超級管理員
  - 驗證：必填

#### 列表欄位顯示
✅ 帳號 - 已停用標記
✅ 電子信箱
✅ 角色 - SuperAdmin 標記為紫色
✅ **管理員等級名稱** - 新增欄位顯示
✅ 狀態 - 停用中 / 首次登入 / 啟用（三種狀態）
✅ 建立時間
✅ 操作

#### 停用按鈕實現（★）
✅ 每列新增「停用」按鈕
✅ 含 JS confirm 確認
✅ IsBlacklisted = true 的列顯示灰底 (bg-gray-100 opacity-60)
✅ IsBlacklisted = true 的列標記「[已停用]」
✅ 自己那列停用按鈕設為 disabled
✅ 已停用的列停用按鈕設為 disabled

#### 成功訊息顯示
✅ 新增成功後顯示生成的帳號
✅ 格式：「新增帳號：ADM001」

#### UI 亮點
✅ 響應式設計（Tailwind CSS）
✅ Modal 彈窗
✅ 顏色區分角色（Blue/Purple）
✅ 狀態標籤色碼（Red/Yellow/Green）
✅ 灰底禁用行效果
✅ 按鈕懸停效果

## 檔案清單

### 已修改
1. `AdminIndexVm.cs` - 新增 CreateForm 和 GeneratedAccount
2. `AdminController.cs` - 新增 CreateAdmin 和 DeactivateAdmin 方法

### 已建立
1. `AdminCreateVm.cs` - 新增管理員表單模型
2. `Index_new.cshtml` - 完整的管理員管理頁面（含 Modal 和停用功能）

## 備註

### Index.cshtml 替換步驟
由於原檔案編碼問題，新 cshtml 文件名為 `Index_new.cshtml`，需手動：
1. 備份原 `Index.cshtml`
2. 將 `Index_new.cshtml` 改名為 `Index.cshtml`

或透過命令行：
```bash
cd ISpanShop.MVC/Areas/Admin/Views/Admin/
ren Index.cshtml Index_old.cshtml
ren Index_new.cshtml Index.cshtml
```

## 流程測試

### 測試流程
1. 導航至 `/Admin/Admin/` 查看列表
2. 點擊「+ 新增管理員」開啟 Modal
3. 選擇等級、輸入密碼、點擊新增
4. 成功後顯示生成的帳號（如 ADM001）
5. 列表刷新後顯示新管理員
6. 對其他管理員點擊「停用」按鈕測試停用功能
7. 停用後列表顯示灰底和「[已停用]」標記
8. 自己那列停用按鈕呈灰色 disabled 狀態

## 技術細節

### Cookie 使用者識別
```csharp
var currentUserId = User.FindFirst("userid")?.Value 
    ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
```

### Modal 動作控制
- 開啟：`openCreateModal()` 移除 `hidden` class
- 關閉：`closeCreateModal(event)` 添加 `hidden` class
- 點擊背景關閉：`onclick="closeCreateModal(event)"` with event.stopPropagation()

### 停用確認對話
```html
onclick="return confirm('確認要停用此管理員嗎？');"
```

### 禁用按鈕樣式
```css
button:disabled {
    opacity: 0.5;
    cursor: not-allowed;
}
```
