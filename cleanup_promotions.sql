-- ============================================
-- 清理促銷活動假資料並新增測試資料
-- ============================================

-- 步驟 1: 查看目前有哪些活動沒有對應到真實賣家
PRINT '===== 步驟 1: 查看沒有有效 SellerId 的活動 ====='
SELECT 
    p.Id, 
    p.Name, 
    p.SellerId,
    p.Status,
    p.CreatedAt
FROM Promotions p
LEFT JOIN Users u ON p.SellerId = u.Id
WHERE u.Id IS NULL OR p.IsDeleted = 1
ORDER BY p.Id;

-- 步驟 2: 刪除沒有對應賣家的假資料
PRINT ''
PRINT '===== 步驟 2: 刪除無效活動 ====='
DELETE FROM PromotionRules WHERE PromotionId IN (
    SELECT p.Id FROM Promotions p
    LEFT JOIN Users u ON p.SellerId = u.Id
    WHERE u.Id IS NULL OR p.IsDeleted = 1
);

DELETE FROM PromotionItems WHERE PromotionId IN (
    SELECT p.Id FROM Promotions p
    LEFT JOIN Users u ON p.SellerId = u.Id
    WHERE u.Id IS NULL OR p.IsDeleted = 1
);

DELETE FROM Promotions 
WHERE Id IN (
    SELECT p.Id FROM Promotions p
    LEFT JOIN Users u ON p.SellerId = u.Id
    WHERE u.Id IS NULL OR p.IsDeleted = 1
);

PRINT '已刪除無效活動'

-- 步驟 3: 查看有哪些賣場（及其對應的賣家 UserId）
PRINT ''
PRINT '===== 步驟 3: 查看現有賣場 ====='
SELECT 
    s.Id AS StoreId,
    s.StoreName,
    s.UserId AS SellerId,
    u.Account AS SellerAccount,
    (SELECT COUNT(*) FROM Products WHERE StoreId = s.Id AND IsDeleted = 0) AS ProductCount
FROM Stores s
JOIN Users u ON s.UserId = u.Id
WHERE s.StoreStatus = 1
ORDER BY s.Id;

-- 步驟 4: 查看目前剩下的活動
PRINT ''
PRINT '===== 步驟 4: 查看剩餘活動 ====='
SELECT 
    p.Id,
    p.Name,
    p.SellerId,
    u.Account AS SellerAccount,
    s.StoreName,
    p.PromotionType,
    p.Status,
    p.StartTime,
    p.EndTime,
    p.CreatedAt
FROM Promotions p
JOIN Users u ON p.SellerId = u.Id
LEFT JOIN Stores s ON s.UserId = u.Id
WHERE p.IsDeleted = 0
ORDER BY p.Id;

-- 步驟 5: 如果活動太少，為每個賣場新增測試活動
PRINT ''
PRINT '===== 步驟 5: 新增測試活動（如果需要）====='

-- 先檢查活動總數
DECLARE @PromotionCount INT;
SELECT @PromotionCount = COUNT(*) FROM Promotions WHERE IsDeleted = 0;

IF @PromotionCount < 5
BEGIN
    PRINT '活動數量不足，開始新增測試資料...'
    
    -- 為每個有商品的賣場新增測試活動
    DECLARE @StoreId INT, @UserId INT, @StoreName NVARCHAR(100);
    
    DECLARE store_cursor CURSOR FOR
    SELECT s.Id, s.UserId, s.StoreName
    FROM Stores s
    WHERE s.StoreStatus = 1 
      AND EXISTS (SELECT 1 FROM Products WHERE StoreId = s.Id AND IsDeleted = 0)
    ORDER BY s.Id;
    
    OPEN store_cursor;
    FETCH NEXT FROM store_cursor INTO @StoreId, @UserId, @StoreName;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- 活動 1: 進行中的限時特賣
        IF NOT EXISTS (
            SELECT 1 FROM Promotions 
            WHERE SellerId = @UserId 
              AND Status = 1 
              AND StartTime <= GETDATE() 
              AND EndTime >= GETDATE()
              AND IsDeleted = 0
        )
        BEGIN
            INSERT INTO Promotions (
                Name, 
                Description, 
                PromotionType, 
                StartTime, 
                EndTime, 
                Status, 
                SellerId, 
                CreatedAt, 
                IsDeleted
            )
            VALUES (
                @StoreName + ' 限時特賣',
                '全館指定商品限時優惠中',
                1,  -- 限時特賣
                DATEADD(DAY, -2, GETDATE()),
                DATEADD(DAY, 5, GETDATE()),
                1,  -- 已核准（進行中）
                @UserId,
                DATEADD(DAY, -3, GETDATE()),
                0
            );
            PRINT '  ✓ 已為 ' + @StoreName + ' 新增「進行中」活動';
        END
        
        -- 活動 2: 待審核的滿額折扣
        IF NOT EXISTS (
            SELECT 1 FROM Promotions 
            WHERE SellerId = @UserId 
              AND Status = 0
              AND IsDeleted = 0
        )
        BEGIN
            INSERT INTO Promotions (
                Name, 
                Description, 
                PromotionType, 
                StartTime, 
                EndTime, 
                Status, 
                SellerId, 
                CreatedAt, 
                IsDeleted
            )
            VALUES (
                @StoreName + ' 滿額優惠',
                '消費滿千折百，滿兩千折三百',
                2,  -- 滿額折扣
                DATEADD(DAY, 3, GETDATE()),
                DATEADD(DAY, 10, GETDATE()),
                0,  -- 待審核
                @UserId,
                DATEADD(HOUR, -6, GETDATE()),
                0
            );
            PRINT '  ✓ 已為 ' + @StoreName + ' 新增「待審核」活動';
        END
        
        -- 活動 3: 即將開始的限量搶購（已核准）
        IF NOT EXISTS (
            SELECT 1 FROM Promotions 
            WHERE SellerId = @UserId 
              AND Status = 1
              AND StartTime > GETDATE()
              AND IsDeleted = 0
        )
        BEGIN
            INSERT INTO Promotions (
                Name, 
                Description, 
                PromotionType, 
                StartTime, 
                EndTime, 
                Status, 
                SellerId, 
                ReviewedBy,
                ReviewedAt,
                CreatedAt, 
                IsDeleted
            )
            VALUES (
                @StoreName + ' 限量搶購',
                '熱銷商品限量搶購，售完為止',
                3,  -- 限量搶購
                DATEADD(DAY, 2, GETDATE()),
                DATEADD(DAY, 8, GETDATE()),
                1,  -- 已核准（即將開始）
                @UserId,
                1,  -- 假設管理員 ID = 1
                DATEADD(HOUR, -12, GETDATE()),
                DATEADD(DAY, -1, GETDATE()),
                0
            );
            PRINT '  ✓ 已為 ' + @StoreName + ' 新增「即將開始」活動';
        END
        
        FETCH NEXT FROM store_cursor INTO @StoreId, @UserId, @StoreName;
    END
    
    CLOSE store_cursor;
    DEALLOCATE store_cursor;
    
    PRINT ''
    PRINT '測試資料新增完成！'
END
ELSE
BEGIN
    PRINT '活動數量充足 (' + CAST(@PromotionCount AS NVARCHAR(10)) + ' 筆)，跳過新增'
END

-- 步驟 6: 最終確認 - 顯示所有活動及其賣場資訊
PRINT ''
PRINT '===== 步驟 6: 最終確認 - 所有活動列表 ====='
SELECT 
    p.Id AS 活動ID,
    p.Name AS 活動名稱,
    u.Account AS 賣家帳號,
    s.StoreName AS 賣場名稱,
    CASE p.PromotionType
        WHEN 1 THEN '限時特賣'
        WHEN 2 THEN '滿額折扣'
        WHEN 3 THEN '限量搶購'
        ELSE '其他'
    END AS 活動類型,
    CASE p.Status
        WHEN 0 THEN '待審核'
        WHEN 1 THEN CASE 
            WHEN p.StartTime > GETDATE() THEN '即將開始'
            WHEN p.EndTime < GETDATE() THEN '已結束'
            ELSE '進行中'
        END
        WHEN 2 THEN '已拒絕'
        ELSE '未知'
    END AS 狀態,
    p.StartTime AS 開始時間,
    p.EndTime AS 結束時間,
    p.CreatedAt AS 建立時間
FROM Promotions p
JOIN Users u ON p.SellerId = u.Id
LEFT JOIN Stores s ON s.UserId = u.Id AND s.StoreStatus = 1
WHERE p.IsDeleted = 0
ORDER BY p.Status, p.StartTime;

PRINT ''
PRINT '===== 統計資訊 ====='
SELECT 
    COUNT(*) AS 總活動數,
    SUM(CASE WHEN Status = 0 THEN 1 ELSE 0 END) AS 待審核,
    SUM(CASE WHEN Status = 1 AND StartTime <= GETDATE() AND EndTime >= GETDATE() THEN 1 ELSE 0 END) AS 進行中,
    SUM(CASE WHEN Status = 1 AND StartTime > GETDATE() THEN 1 ELSE 0 END) AS 即將開始,
    SUM(CASE WHEN Status = 2 THEN 1 ELSE 0 END) AS 已拒絕,
    SUM(CASE WHEN Status = 1 AND EndTime < GETDATE() THEN 1 ELSE 0 END) AS 已結束
FROM Promotions
WHERE IsDeleted = 0;

PRINT ''
PRINT '✅ 資料清理與測試資料新增完成！'
