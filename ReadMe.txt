# ISpanShop 電商後台管理系統

## 專案資訊
- 技術棧：ASP.NET Core 8.0 MVC
- 資料庫：SQL Server
- 架構：五層架構 (Common → Models → Repositories → Services → MVC/WebAPI)

## AI 協作指示

請先掃描整個專案檔案結構。

## 任務 1：修改商品自動生成程式碼，讓商品對應正確賣場

### 第一步：找出自動生成商品的程式碼
搜尋專案中所有跟自動生成商品、Seed、ProductSeeder、GenerateProducts、SeedProducts、測試商品 相關的檔案，列出每個檔案的完整路徑和目前 StoreId 是怎麼設定的。
不要修改圖片相關的邏輯，只改 StoreId 的分配。

### 第二步：修改 StoreId 分配邏輯
目前資料庫中的 8 個賣場如下，請根據分類對應賣場：

Store 1 - 小明の奇妙雜貨
→ CategoryId=7（生鮮食材與飲品）
→ CategoryId=8（居家裝飾與收納）
→ CategoryId=9（廚房餐具與用品）

Store 2 - 建國五金與生活百貨
→ CategoryId=5（大型家具）
→ CategoryId=8（居家裝飾與收納）
→ CategoryId=9（廚房餐具與用品）

Store 3 - 豪客3C數位館
→ CategoryId=11（筆記型電腦）
→ CategoryId=16（手機與平板周邊）
→ CategoryId=20（智慧型手機）
→ CategoryId=24（平板電腦）

Store 4 - 秘密衣櫥
→ CategoryId=13（男款上衣與襯衫）
→ CategoryId=14（男士鞋款）
→ CategoryId=26（女款上衣與洋裝）
→ CategoryId=28（精品包包）
→ CategoryId=29（派對與晚禮服）
→ CategoryId=31（女款鞋類）
→ CategoryId=32（女士腕錶）

Store 5 - 手作香噴噴工坊
→ CategoryId=3（香水與香氛）
→ CategoryId=19（臉部與身體保養）

Store 6 - 心怡日韓嚴選代購
→ CategoryId=2（彩妝與修容）
→ CategoryId=3（香水與香氛）
→ CategoryId=19（臉部與身體保養）

Store 7 - 宇過天晴文創選物
→ CategoryId=8（居家裝飾與收納）
→ CategoryId=15（男士腕錶）
→ CategoryId=30（珠寶與飾品）

Store 8 - 流浪戶外露營裝備
→ CategoryId=22（運動裝備與球類）
→ CategoryId=23（太陽眼鏡與配件）

### 修改方式
在生成商品的程式碼中，根據商品的 CategoryId 來決定 StoreId：
1. 建立一個 CategoryId → StoreId 的對應字典（Dictionary 或 Map）
2. CategoryId=8 和 CategoryId=9 有多個賣場可選（Store 1 和 Store 2），可以隨機分配或輪流分配
3. CategoryId=3 和 CategoryId=19 也有多個賣場可選（Store 5 和 Store 6），同樣處理
4. 生成商品時根據 CategoryId 查字典取得對應的 StoreId

### 預期結果
修改完成後，每個賣場應該只包含上述對應的分類商品，不會出現不相關的商品類別。

### 重要注意事項
- ❌ 不要動圖片相關的程式碼，只改 StoreId 的分配邏輯
- ❌ 不要動商品名稱、價格、規格等其他生成邏輯
- ✅ 改完後重新執行生成，確認各賣場都有對應分類的商品
- ✅ 用以下 SQL 驗證結果：

```sql
SELECT s.StoreName, c.Name AS Category, COUNT(p.Id) AS ProductCount
FROM Products p 
JOIN Stores s ON p.StoreId = s.Id 
JOIN Categories c ON p.CategoryId = c.Id
GROUP BY s.StoreName, c.Name
ORDER BY s.StoreName, c.Name;
```

## 任務 2：修復側邊面板展開時待審核卡片跳轉問題

### 問題描述
商品總覽頁面，當側邊面板展開時，點擊「待審核」卡片無法跳轉到商品審核中心頁面。

### 解決方案
不管側邊面板是否展開，點擊「待審核」卡片都要跳轉到商品審核中心頁面。

### 修改步驟
1. 找到待審核卡片的 click 事件程式碼
2. 在 click handler 的最開頭直接執行跳轉，不要被任何其他邏輯擋住：
   ```javascript
   window.location.href = '/Admin/ProductReview'; // 或對應的路由
   ```
3. 不要有任何 if 判斷面板狀態的邏輯
4. 加上除錯訊息：
   ```javascript
   console.log('待審核卡片被點擊，準備跳轉');
   ```
5. 確認跳轉 URL 是正確的商品審核中心路徑

### 測試驗證
- 側邊面板關閉時點擊待審核卡片 → 應跳轉
- 側邊面板展開時點擊待審核卡片 → 應跳轉

## 整體執行步驟
1. ✅ 掃描專案找出商品生成相關檔案
2. ✅ 只修改 StoreId 分配邏輯（不動圖片、名稱、價格）
3. ✅ 重新生成商品資料
4. ✅ 用 SQL 查詢驗證各賣場商品分類正確
5. ✅ 修復待審核卡片跳轉問題
6. ✅ 測試兩種狀態（面板開/關）確認都能正常跳轉

---
最後更新：2026-03-16