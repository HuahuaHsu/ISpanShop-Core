# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

> 上層架構說明見根目錄 `CLAUDE.md`。本文件聚焦在 `ISpanShop.MVC` 後端 API 開發細節。

---

## 目錄分工

| 目錄 | 用途 | 狀態 |
|---|---|---|
| `Areas/Admin/Controllers/` | 後台 Razor MVC，Cookie 認證，回傳 `View` / Redirect | ✅ 已完成，**勿動** |
| `Areas/Admin/Models/` | 後台 ViewModel | ✅ 已完成，**勿動** |
| `Controllers/Api/` | 前台 REST API，回傳 JSON，**正在開發** | 🔧 開發中 |
| `Controllers/` (根層) | 特殊流程控制器（結帳、金流 callback）| 視需求異動 |
| `Models/` | 前台用的 ViewModel / Request DTO | 🔧 開發中 |
| `Middleware/` | 全域 middleware 與授權 filter（見下方說明）| 通常不需動 |

### Middleware 說明

| 檔案 | 用途 |
|---|---|
| `ExceptionHandlingMiddleware.cs` | 全域例外捕捉：開發環境拋出（黃頁）；正式環境回傳 JSON `{ message }` + HTTP 500 |
| `RequirePermissionAttribute.cs` | Admin 細粒度權限 filter，`[RequirePermission("key")]`；超級管理員（`AdminLevelId=1`）自動略過；未登入導向 `/Admin/Auth/Login` |
| `RequireSuperAdminAttribute.cs` | 限制只有超級管理員（`AdminLevelId=1`）可存取 |

---

## ⚠️ JWT 認證尚未實作

`Program.cs` 目前**只有 Cookie 認證**（`AdminCookieAuth`），供後台使用。
前台 API 的 JWT 認證**尚未加入**。
需要實作時，在 `Program.cs` 補上 `AddJwtBearer`，並確認不影響現有 Cookie 認證流程。

---

## 前台 API 開發慣例

### 路由格式
```
GET  /api/products              # 列表（含分頁、篩選）
GET  /api/products/{id}         # 單筆詳情
POST /api/seller/products       # 賣家建立（需身份）
PUT  /api/seller/products/{id}  # 賣家編輯
DEL  /api/seller/products/{id}  # 賣家刪除
```

### Controller 命名慣例
- 檔名：`{Domain}ApiController.cs`（公開）/ `Seller{Domain}ApiController.cs`（需賣家身份）
- 放在：`Controllers/Api/{Domain}/`
- 標記：`[ApiController]`、`[Route("api/...")]`、`[Produces("application/json")]`
- 每個 action 加 `[ProducesResponseType]` 供 Swagger 文件

### 統一回傳格式（目標慣例）
```json
{ "success": true, "data": {}, "message": "" }
```
錯誤時使用對應 HTTP status code（400、401、403、404、500），不要全部回 200。

### CORS
- 開發環境：`AllowVite` policy，允許 `http://localhost:5173`，支援 `AllowCredentials`
- 正式環境：`FrontendPolicy`，AllowAnyOrigin（上線前請鎖定網域）

---

## 多規格商品資料結構

規格設計採 **JSON 儲存**，無獨立 Spec / SpecOption 資料表。

```
Product
 ├─ SpecDefinitionJson    # JSON，定義規格軸名稱，例如 ["顏色","尺寸"]
 ├─ MinPrice / MaxPrice   # 從 ProductVariants 彙總的最低/最高價（反正規化）
 └─ ProductVariants[]
      ├─ SkuCode           # 唯一 SKU 編號
      ├─ VariantName       # 人類可讀名稱，例如 "紅色-M"
      ├─ SpecValueJson     # JSON，對應 SpecDefinitionJson 的值，例如 ["紅色","M"]
      ├─ Price             # 此 SKU 售價（價格在 Variant 層）
      ├─ Stock             # 庫存數量（庫存在 Variant 層）
      └─ SafetyStock       # 安全庫存警示閾值
```

分類屬性（`CategoryAttribute` → `CategoryAttributeOption`）是商品的「規格以外的屬性」（如品牌、材質），透過 `CategoryAttributeMapping` 與分類綁定，與上方規格是不同概念。

---

## 已完成的前台 API 端點

> 每完成一個端點請補在下方

| Method | Route | Controller | 說明 |
|---|---|---|---|
| GET | `/api/products`           | `Controllers/Api/Products/ProductsApiController`      | 前台商品總覽（AllowAnonymous）；支援 categoryId / keyword / sortBy / page / pageSize |
| GET | `/api/categories`         | `Controllers/Api/Categories/CategoryApiController`    | 前台主分類列表（AllowAnonymous）；回傳啟用中主分類 + 上架商品數 |
| GET | `/api/promotions/active`  | `Controllers/Api/Promotions/PromotionApiController`   | 前台進行中活動（AllowAnonymous）；支援 type / limit |
| GET | `/api/products/{id}`      | `Controllers/Api/Products/ProductsApiController`      | 前台商品詳情（AllowAnonymous）；含規格/圖片/品牌/商店/分類路徑/評分 |
| GET | `/api/products/{id}/related` | `Controllers/Api/Products/ProductsApiController`   | 前台相關商品（AllowAnonymous）；同子分類依銷量排序，支援 limit |
