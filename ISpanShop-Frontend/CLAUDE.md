# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 技術棧（強制，不得自行替換）

| 技術 | 用途 | 備註 |
|---|---|---|
| Vue 3 + `<script setup>` | 元件開發，**必須用 Composition API** | 禁用 Options API |
| Vite | 建構工具 | 設定在 `vite.config.ts` |
| TypeScript strict | **所有變數、函式參數、回傳值都要標型別** | `noUncheckedIndexedAccess` 已啟用 |
| Element Plus | 按鈕、表單、表格、分頁、訊息提示 — **一律用 EP，不自刻** | 語系：`zh-tw` |
| Axios | AJAX，**不用原生 fetch** | ⚠️ 尚未安裝，需先 `npm i axios` |
| Pinia | 狀態管理（已安裝 v3） | Composition API 風格 store |
| Vue Router | 路由（已安裝 v5） | `createWebHistory` |

## 指令

```bash
npm run dev        # 開發伺服器 port 5173
npm run build      # type-check + 打包
npm run lint       # oxlint + eslint --fix
npm run format     # Prettier 格式化 src/
npm run type-check # 單獨跑 vue-tsc
```

## 目錄結構

```
src/
├── api/          # Axios 封裝 + API 呼叫函式，照功能分檔（products.ts、auth.ts…）
├── assets/       # 全域 CSS、圖片
├── components/   # 跨頁面可重用元件（PascalCase 命名）
├── composables/  # 組合式函式，use 開頭（useProductSpec.ts…）
├── constants/    # 全域常數
├── layouts/      # DefaultLayout / MemberLayout / BlankLayout（已完成）
├── router/       # index.ts — 路由設定
├── stores/       # Pinia store，一功能一檔
├── types/        # TS interface / type（後端 DTO 對應定義放這裡）
├── utils/        # 純函式工具
└── views/        # 頁面元件，對應 router routes
    └── seller/   # 賣家後台頁面
```

## Axios 封裝慣例

封裝檔：`src/api/request.ts`（尚未建立，開發前請先建）

必須實作：
- `baseURL`：從 `.env` 的 `VITE_API_BASE_URL` 讀取
- **請求攔截器**：自動從 store / localStorage 讀取 JWT，加入 `Authorization: Bearer <token>`
- **回應攔截器**：
  - HTTP 401 → 清除 token，`router.push('/auth/login')`
  - 其他錯誤 → `ElMessage.error(錯誤訊息)`

API 函式放 `src/api/<功能>.ts`，命名用動詞開頭：`fetchProducts`、`createProduct`、`updateProduct`、`deleteProduct`。

## TypeScript 型別對齊

後端 DTO 對應的 interface 一律放 `src/types/`，與後端 `ISpanShop.Models/DTOs/` 保持同步。

命名慣例：

| 用途 | 命名範例 |
|---|---|
| 列表項目 | `ProductListItem` |
| 詳情 | `ProductDetailDto` |
| 建立請求 | `CreateProductRequest` |
| 更新請求 | `UpdateProductRequest` |
| 分頁包裝 | `PagedResult<T>` |
| 統一回傳 | `ApiResponse<T>` （`{ success, data, message }`）|

## 路由與 Layout

```
/              → DefaultLayout  (頁首 + 分類列 + 頁尾)
/member/*      → MemberLayout   (側邊選單)
/auth/*        → BlankLayout    (極簡)
```

新增頁面：在對應 layout 的 `children` 加 route，view 放對應目錄。

## 待辦頁面清單

| 檔案 | 功能 | 狀態 |
|---|---|---|
| `views/HomeView.vue` | 首頁商品列表：分類篩選、排序、分頁 | 有 UI 殼，待串 API |
| `views/ProductDetailView.vue` | 商品詳情：多規格選擇器 | 待建立 |
| `views/seller/ProductManageView.vue` | 賣家商品管理（列表、刪除）| 待建立 |
| `views/seller/ProductEditView.vue` | 新增 / 編輯商品表單 | 待建立 |

## 多規格選擇器 UX 規則

規格資料來源：`Product.SpecDefinitionJson`（軸名）+ `ProductVariant.SpecValueJson`（各 SKU 的值）

實作規則：
1. 選了規格 A 後，過濾剩餘 variants，根據庫存把規格 B 的無效選項設為 `disabled`
2. 切換任一規格 → 價格、庫存數量、主圖同步更新
3. 庫存 = 0 的 variant → 顯示「售完」，**不隱藏按鈕**
4. 尚未選完所有規格時，加入購物車按鈕維持 disabled

## 命名慣例

| 類型 | 慣例 | 範例 |
|---|---|---|
| 元件檔 | PascalCase | `ProductCard.vue`、`SpecSelector.vue` |
| 組合式函式 | `use` 開頭 | `useProductSpec`、`useCart` |
| API 函式 | 動詞開頭 | `fetchProducts`、`createProduct` |
| Pinia store | `use` + 名詞 + `Store` | `useProductStore` |
| TS interface | 同後端 DTO 名稱 | `ProductDetailDto` |
