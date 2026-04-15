# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 專案說明

HowBuy (ISpanShop) 是一個類蝦皮的電商平台。本分支負責**商品模組**，包含：

- 首頁商品列表（分類篩選）
- 商品詳情頁（多規格選擇）
- 賣家商品管理（新增 / 編輯 / 刪除）

## 啟動指令

```bash
# 後端 (port 7125)
dotnet run --project ISpanShop.MVC

# 前端 (port 5173)
cd ISpanShop-Frontend && npm run dev
```

其他前端指令：`npm run build` / `npm run lint` / `npm run type-check`

## 架構總覽

| 層 | 專案 | 職責 |
|---|---|---|
| 共用工具 | `ISpanShop.Common` | Enums、Helper（ECPay、Security 等） |
| 資料模型 | `ISpanShop.Models` | EF Core entities、DTOs、DataSeeder |
| 資料存取 | `ISpanShop.Repositories` | EF Core（大多數）/ ADO.NET（Admin 相關） |
| 商業邏輯 | `ISpanShop.Services` | 業務規則、entity → DTO 轉換 |
| 呈現層 | `ISpanShop.MVC` | 後台 Razor MVC（`Areas/Admin/`）+ 前台 REST API（`Controllers/Api/`） |
| 前台 SPA | `ISpanShop-Frontend` | Vue 3 + TypeScript，呼叫 `/api` 端點，UI 使用 Element Plus |

Vite proxy：前端 `/api/*` → `https://localhost:7125`

---

## ⚠️ 共用層修改警告

**Services / Repositories / Models 同時被後台（Areas/Admin）和前台 API 共用。**

修改任何 Service 方法之前，必須先執行：
```bash
grep -r "方法名稱" ISpanShop.MVC/Areas/Admin/Controllers/
```
確認後台是否正在使用。

**如果前台需要不同的邏輯 → 新增方法，不要改舊方法。**
改舊方法可能靜默破壞已完成的後台功能，且不會有編譯錯誤提示。

---

## ⚠️ 認證分流警告

| 區域 | 認證方式 |
|---|---|
| `Areas/Admin/` 後台 | Cookie 認證 |
| `Controllers/Api/` 前台 | JWT 認證 |

修改 `Program.cs` 的 middleware（認證、授權、CORS 等）時，**兩套認證都必須顧到**，不可只處理其中一套。

---

## ⚠️ 後台已完成，請勿隨意修改

`Areas/Admin/` 下的控制器、ViewModel、Views 已完成開發。
除非任務明確要求，否則不要異動後台程式碼。
